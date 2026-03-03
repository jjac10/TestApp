using TestApp.Core.Models;

namespace TestApp.Core.Services;

public interface IDeckService
{
    Task<List<Deck>> GetAllDecksAsync(string userId);
    Task<Deck> CreateDeckAsync(string name, string userId);
    Task DeleteDeckAsync(int deckId, string userId);
    Task<Deck?> GetDeckWithFilesAsync(int deckId, string userId);
}