namespace TestApp.Api.DTOs;

public class PdfPreviewItem
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string? StoredFileName { get; set; }
    public int QuestionCount { get; set; }
    public bool IsValid { get; set; }
    public string? Error { get; set; }
}

public class PdfPreviewResponse
{
    public string SessionId { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public int ValidFiles { get; set; }
    public int TotalQuestions { get; set; }
    public List<PdfPreviewItem> Files { get; set; } = [];
}

public class PdfImportResult
{
    public string FileName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public string? Error { get; set; }
}
