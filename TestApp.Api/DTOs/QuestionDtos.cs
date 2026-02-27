using TestApp.Core.DTOs;

namespace TestApp.Api.DTOs;

public class RecordAnswerRequest
{
    public char UserAnswer { get; set; }
}

public class UpdateCorrectAnswerRequest
{
    public char NewCorrectAnswer { get; set; }
}

public class ImportQuestionsRequest
{
    public string FileName { get; set; } = string.Empty;
    public List<QuestionImportDto> Questions { get; set; } = [];
}
