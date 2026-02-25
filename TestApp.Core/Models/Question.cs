namespace TestApp.Core.Models;

public class Question
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string Statement { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public char CorrectAnswer { get; set; }
    public string? Source { get; set; }

    public int FileId { get; set; }
    public QuestionFile File { get; set; } = null!;

    public ICollection<Answer> Answers { get; set; } = [];

    // Computed properties for statistics
    public int TimesAnswered => Answers.Count;
    public int TimesCorrect => Answers.Count(a => a.IsCorrect);
    public int TimesFailed => Answers.Count(a => !a.IsCorrect);
    public bool NeverAnswered => Answers.Count == 0;
}