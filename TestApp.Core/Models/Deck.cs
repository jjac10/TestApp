namespace TestApp.Core.Models;

public class Deck
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public ICollection<QuestionFile> Files { get; set; } = [];
}