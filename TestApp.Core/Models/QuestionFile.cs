namespace TestApp.Core.Models;

public class QuestionFile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; } = DateTime.Now;

    public int DeckId { get; set; }
    public Deck Deck { get; set; } = null!;

    public ICollection<Question> Questions { get; set; } = [];
}