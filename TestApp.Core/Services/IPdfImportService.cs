using TestApp.Core.DTOs;

namespace TestApp.Core.Services;

public interface IPdfImportService
{
    Task<List<QuestionImportDto>> ExtractQuestionsFromPdfAsync(string pdfPath, int numberOfQuestions);
    Task<int> CountQuestionsInPdfAsync(string pdfPath);
}