using Microsoft.AspNetCore.Identity;

namespace TestApp.Core.Models;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Deck> Decks { get; set; } = [];
}