using Microsoft.AspNetCore.Mvc;
using TestApp.Api.DTOs;
using TestApp.Core.DTOs;
using TestApp.Core.Services;

namespace TestApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    /// <summary>
    /// Obtiene preguntas para un examen de un mazo
    /// </summary>
    [HttpGet("deck/{deckId}/exam")]
    public async Task<IActionResult> GetForExam(int deckId, [FromQuery] int count = 10, [FromQuery] QuestionFilter filter = QuestionFilter.All)
    {
        var questions = await _questionService.GetQuestionsForExamAsync(deckId, count, filter);
        return Ok(questions);
    }

    /// <summary>
    /// Obtiene preguntas para un examen de un archivo
    /// </summary>
    [HttpGet("file/{fileId}/exam")]
    public async Task<IActionResult> GetFromFile(int fileId, [FromQuery] int count = 10, [FromQuery] QuestionFilter filter = QuestionFilter.All)
    {
        var questions = await _questionService.GetQuestionsFromFileAsync(fileId, count, filter);
        return Ok(questions);
    }

    /// <summary>
    /// Obtiene todas las preguntas de un archivo ordenadas
    /// </summary>
    [HttpGet("file/{fileId}")]
    public async Task<IActionResult> GetAllFromFile(int fileId)
    {
        var questions = await _questionService.GetAllQuestionsFromFileOrderedAsync(fileId);
        return Ok(questions);
    }

    /// <summary>
    /// Cuenta preguntas de un archivo según filtro
    /// </summary>
    [HttpGet("file/{fileId}/count")]
    public async Task<IActionResult> CountInFile(int fileId, [FromQuery] QuestionFilter filter = QuestionFilter.All)
    {
        var count = await _questionService.CountQuestionsInFileAsync(fileId, filter);
        return Ok(count);
    }

    /// <summary>
    /// Cuenta preguntas de un mazo según filtro
    /// </summary>
    [HttpGet("deck/{deckId}/count")]
    public async Task<IActionResult> CountInDeck(int deckId, [FromQuery] QuestionFilter filter = QuestionFilter.All)
    {
        var count = await _questionService.CountQuestionsInDeckAsync(deckId, filter);
        return Ok(count);
    }

    /// <summary>
    /// Registra una respuesta del usuario
    /// </summary>
    [HttpPost("{questionId}/answer")]
    public async Task<IActionResult> RecordAnswer(int questionId, [FromBody] RecordAnswerRequest request)
    {
        await _questionService.RecordAnswerAsync(questionId, request.UserAnswer);
        return Ok();
    }

    /// <summary>
    /// Actualiza la respuesta correcta de una pregunta
    /// </summary>
    [HttpPut("{questionId}/correct-answer")]
    public async Task<IActionResult> UpdateCorrectAnswer(int questionId, [FromBody] UpdateCorrectAnswerRequest request)
    {
        await _questionService.UpdateCorrectAnswerAsync(questionId, request.NewCorrectAnswer);
        return Ok();
    }

    /// <summary>
    /// Elimina un archivo y sus preguntas
    /// </summary>
    [HttpDelete("file/{fileId}")]
    public async Task<IActionResult> DeleteFile(int fileId)
    {
        await _questionService.DeleteFileAsync(fileId);
        return NoContent();
    }

    /// <summary>
    /// Elimina una pregunta de un archivo
    /// </summary>
    [HttpDelete("question/{questionId}")]
    public async Task<IActionResult> DeleteQuestion(int questionId)
    {
        await _questionService.DeleteQuestionAsync(questionId);
        return NoContent();
    }

    /// <summary>
    /// Importa preguntas desde un JSON a un mazo
    /// </summary>
    [HttpPost("deck/{deckId}/import")]
    public async Task<IActionResult> ImportQuestions(int deckId, [FromBody] ImportQuestionsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            return BadRequest("El nombre del archivo es requerido");

        if (request.Questions == null || request.Questions.Count == 0)
            return BadRequest("No se proporcionaron preguntas");

        var count = await _questionService.ImportQuestionsAsync(deckId, request.FileName, request.Questions);
        return Ok(new { ImportedCount = count });
    }
}
