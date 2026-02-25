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
            .AsNoTracking()
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
        // Usar SQL directo para eliminar en cascada de forma segura
        // Primero eliminar respuestas
        await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM Answers 
            WHERE QuestionId IN (
                SELECT q.Id FROM Questions q
                INNER JOIN QuestionFiles f ON q.FileId = f.Id
                WHERE f.DeckId = {0}
            )", deckId);
        
        // Luego eliminar preguntas
        await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM Questions 
            WHERE FileId IN (
                SELECT Id FROM QuestionFiles WHERE DeckId = {0}
            )", deckId);
        
        // Luego eliminar archivos
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM QuestionFiles WHERE DeckId = {0}", deckId);
        
        // Finalmente eliminar el mazo
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Decks WHERE Id = {0}", deckId);
    }

    public async Task<Deck?> GetDeckWithFilesAsync(int deckId)
        => await _context.Decks
            .AsNoTracking()
            .Include(d => d.Files)
                .ThenInclude(f => f.Questions)
            .FirstOrDefaultAsync(d => d.Id == deckId);
}