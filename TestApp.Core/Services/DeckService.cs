using Microsoft.EntityFrameworkCore;
using TestApp.Core.Data;
using TestApp.Core.Models;

namespace TestApp.Core.Services;

public class DeckService : IDeckService
{
    private readonly AppDbContext _context;

    public DeckService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Deck>> GetAllDecksAsync()
        => await _context.Decks
            .Include(d => d.Files)
                .ThenInclude(f => f.Questions)
            .ToListAsync();

    public async Task<Deck> CreateDeckAsync(string name)
    {
        var deck = new Deck { Name = name };
        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();
        return deck;
    }

    public async Task DeleteDeckAsync(int deckId)
    {
        var deck = await _context.Decks.FindAsync(deckId);
        if (deck != null)
        {
            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Deck?> GetDeckWithFilesAsync(int deckId)
        => await _context.Decks
            .Include(d => d.Files)
                .ThenInclude(f => f.Questions)
            .FirstOrDefaultAsync(d => d.Id == deckId);
}