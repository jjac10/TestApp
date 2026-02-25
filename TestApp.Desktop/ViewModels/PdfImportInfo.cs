namespace TestApp.Desktop.ViewModels;

public class PdfImportInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int DetectedQuestions { get; set; }
    public bool IsDuplicate { get; set; }
}
