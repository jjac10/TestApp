using TestApp.Core.Models;

namespace TestApp.Core.Services;

public interface IDeckService
{
    Task<List<Deck>> GetAllDecksAsync();
    Task<Deck> CreateDeckAsync(string name);
    Task DeleteDeckAsync(int deckId);
    Task<Deck?> GetDeckWithFilesAsync(int deckId);
}