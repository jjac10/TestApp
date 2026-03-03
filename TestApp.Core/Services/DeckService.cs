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

    public async Task<List<Deck>> GetAllDecksAsync(string userId)
    {
        return await _context.Decks
            .Where(d => d.UserId == userId)
            .Include(d => d.Files)
                .ThenInclude(f => f.Questions)
            .ToListAsync();
    }

    public async Task<Deck> CreateDeckAsync(string name, string userId)
    {
        var deck = new Deck { Name = name, UserId = userId };
        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();
        return deck;
    }

    public async Task DeleteDeckAsync(int deckId, string userId)
    {
        var deck = await _context.Decks
            .FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId)
            ?? throw new InvalidOperationException("Mazo no encontrado");

        _context.Decks.Remove(deck);
        await _context.SaveChangesAsync();
    }

    public async Task<Deck?> GetDeckWithFilesAsync(int deckId, string userId)
    {
        return await _context.Decks
            .Where(d => d.Id == deckId && d.UserId == userId)
            .Include(d => d.Files)
                .ThenInclude(f => f.Questions)
            .FirstOrDefaultAsync();
    }
}