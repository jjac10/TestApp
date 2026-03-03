using Microsoft.AspNetCore.Mvc;
using TestApp.Api.DTOs;
using TestApp.Core.Services;

namespace TestApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private static readonly string TempUploadFolder = Path.Combine(Path.GetTempPath(), "TestApp_Uploads");

    private readonly IPdfImportService _pdfImportService;
    private readonly IQuestionService _questionService;

    public ImportController(IPdfImportService pdfImportService, IQuestionService questionService)
    {
        _pdfImportService = pdfImportService;
        _questionService = questionService;
    }

    /// <summary>
    /// Sube un PDF y lo importa directamente a un mazo (sin preview)
    /// </summary>
    [HttpPost("pdf/{deckId}")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> ImportPdf(int deckId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No se proporcionó un archivo");

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Solo se permiten archivos PDF");

        var tempPath = Path.GetTempFileName();
        try
        {
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var questionCount = await _pdfImportService.CountQuestionsInPdfAsync(tempPath);
            if (questionCount == 0)
                return BadRequest("No se encontraron preguntas en el PDF");

            var questions = await _pdfImportService.ExtractQuestionsFromPdfAsync(tempPath, questionCount);
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var importedCount = await _questionService.ImportQuestionsAsync(deckId, fileName, questions);

            return Ok(new { ImportedCount = importedCount, FileName = fileName });
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
                System.IO.File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Paso 1: Sube PDFs, los guarda temporalmente y devuelve un preview con el conteo de preguntas.
    /// El usuario revisa el resumen y confirma con el endpoint confirm.
    /// </summary>
    [HttpPost("pdf/preview")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB total
    public async Task<IActionResult> PreviewPdfs(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No se proporcionaron archivos");

        var sessionId = Guid.NewGuid().ToString("N");
        var sessionFolder = Path.Combine(TempUploadFolder, sessionId);
        Directory.CreateDirectory(sessionFolder);

        var previews = new List<PdfPreviewItem>();

        foreach (var file in files)
        {
            var preview = new PdfPreviewItem { OriginalFileName = file.FileName };

            if (file.Length == 0)
            {
                preview.Error = "El archivo está vacío";
                previews.Add(preview);
                continue;
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                preview.Error = "Solo se permiten archivos PDF";
                previews.Add(preview);
                continue;
            }

            // Guardar con nombre seguro en la carpeta de sesión
            var safeFileName = $"{previews.Count}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(sessionFolder, safeFileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var questionCount = await _pdfImportService.CountQuestionsInPdfAsync(filePath);

                preview.StoredFileName = safeFileName;
                preview.QuestionCount = questionCount;
                preview.IsValid = questionCount > 0;

                if (questionCount == 0)
                    preview.Error = "No se encontraron preguntas en el PDF";
            }
            catch (Exception ex)
            {
                preview.Error = ex.Message;

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            previews.Add(preview);
        }

        return Ok(new PdfPreviewResponse
        {
            SessionId = sessionId,
            TotalFiles = files.Count,
            ValidFiles = previews.Count(p => p.IsValid),
            TotalQuestions = previews.Where(p => p.IsValid).Sum(p => p.QuestionCount),
            Files = previews
        });
    }

    /// <summary>
    /// Paso 2: Confirma la importación de los PDFs previamente subidos en el preview.
    /// </summary>
    [HttpPost("pdf/{deckId}/confirm/{sessionId}")]
    public async Task<IActionResult> ConfirmImport(int deckId, string sessionId)
    {
        var sessionFolder = Path.Combine(TempUploadFolder, sessionId);

        if (!Directory.Exists(sessionFolder))
            return BadRequest("Sesión no encontrada o expirada");

        try
        {
            var pdfFiles = Directory.GetFiles(sessionFolder, "*.pdf");
            if (pdfFiles.Length == 0)
                return BadRequest("No se encontraron archivos en la sesión");

            var results = new List<PdfImportResult>();

            foreach (var filePath in pdfFiles)
            {
                var originalName = Path.GetFileNameWithoutExtension(filePath);
                // Quitar el prefijo numérico "0_"
                var underscoreIndex = originalName.IndexOf('_');
                if (underscoreIndex >= 0)
                    originalName = originalName[(underscoreIndex + 1)..];

                var result = new PdfImportResult { FileName = originalName };

                try
                {
                    var questionCount = await _pdfImportService.CountQuestionsInPdfAsync(filePath);
                    if (questionCount == 0)
                    {
                        result.Success = false;
                        result.Error = "No se encontraron preguntas en el PDF";
                        results.Add(result);
                        continue;
                    }

                    var questions = await _pdfImportService.ExtractQuestionsFromPdfAsync(filePath, questionCount);
                    var importedCount = await _questionService.ImportQuestionsAsync(deckId, originalName, questions);

                    result.Success = true;
                    result.ImportedCount = importedCount;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                }

                results.Add(result);
            }

            var totalImported = results.Where(r => r.Success).Sum(r => r.ImportedCount);
            return Ok(new
            {
                TotalFiles = results.Count,
                SuccessCount = results.Count(r => r.Success),
                FailedCount = results.Count(r => !r.Success),
                TotalImportedQuestions = totalImported,
                Results = results
            });
        }
        finally
        {
            // Limpiar la carpeta de sesión
            if (Directory.Exists(sessionFolder))
                Directory.Delete(sessionFolder, recursive: true);
        }
    }

    /// <summary>
    /// Cancela una sesión de preview y elimina los archivos temporales
    /// </summary>
    [HttpDelete("pdf/preview/{sessionId}")]
    public IActionResult CancelPreview(string sessionId)
    {
        var sessionFolder = Path.Combine(TempUploadFolder, sessionId);

        if (Directory.Exists(sessionFolder))
            Directory.Delete(sessionFolder, recursive: true);

        return NoContent();
    }
}
