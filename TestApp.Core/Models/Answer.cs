namespace TestApp.Core.Models;

public class Answer
{
    public int Id { get; set; }
    public char UserAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.Now;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}