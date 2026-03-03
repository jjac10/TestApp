using TestApp.Core.Models;
using TestApp.Core.Services;
using TestApp.Tests.Helpers;

namespace TestApp.Tests.Services;

public class DeckServiceTests : IDisposable
{
    private const string UserId = TestDbContextFactory.TestUserId;
    private const string OtherUserId = "other-user";
    private readonly TestApp.Core.Data.AppDbContext _context;
    private readonly DeckService _sut;

    public DeckServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _sut = new DeckService(_context);
        SeedOtherUser();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedOtherUser()
    {
        if (_context.Users.Find(OtherUserId) != null) return;

        _context.Users.Add(new User
        {
            Id = OtherUserId,
            UserName = "other",
            NormalizedUserName = "OTHER",
            Email = "other@test.com",
            NormalizedEmail = "OTHER@TEST.COM",
            EmailConfirmed = true,
            FullName = "Other User",
            SecurityStamp = System.Guid.NewGuid().ToString()
        });
        _context.SaveChanges();
    }

    // --- CRUD básico ---

    [Fact]
    public async Task CreateDeckAsync_CreaCorrectamente()
    {
        var deck = await _sut.CreateDeckAsync("Convocatoria 2025", UserId);

        Assert.NotEqual(0, deck.Id);
        Assert.Equal("Convocatoria 2025", deck.Name);
        Assert.Equal(UserId, deck.UserId);
    }

    [Fact]
    public async Task GetAllDecksAsync_DevuelveTodos()
    {
        await _sut.CreateDeckAsync("Deck 1", UserId);
        await _sut.CreateDeckAsync("Deck 2", UserId);

        var decks = await _sut.GetAllDecksAsync(UserId);

        Assert.Equal(2, decks.Count);
    }

    [Fact]
    public async Task GetDeckWithFilesAsync_DevuelveDeckConArchivos()
    {
        var deck = await _sut.CreateDeckAsync("Mi Deck", UserId);
        _context.QuestionFiles.Add(new QuestionFile { Name = "Tema 1", DeckId = deck.Id });
        await _context.SaveChangesAsync();

        var result = await _sut.GetDeckWithFilesAsync(deck.Id, UserId);

        Assert.NotNull(result);
        Assert.Single(result.Files);
        Assert.Equal("Tema 1", result.Files.First().Name);
    }

    [Fact]
    public async Task GetDeckWithFilesAsync_IdInexistente_DevuelveNull()
    {
        var result = await _sut.GetDeckWithFilesAsync(999, UserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteDeckAsync_EliminaCorrectamente()
    {
        var deck = await _sut.CreateDeckAsync("Para borrar", UserId);

        await _sut.DeleteDeckAsync(deck.Id, UserId);

        var decks = await _sut.GetAllDecksAsync(UserId);
        Assert.Empty(decks);
    }

    [Fact]
    public async Task DeleteDeckAsync_IdInexistente_LanzaExcepcion()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.DeleteDeckAsync(999, UserId));
    }

    // --- Aislamiento por usuario ---

    [Fact]
    public async Task GetAllDecksAsync_FiltraPorUsuario()
    {
        await _sut.CreateDeckAsync("Deck Usuario A", UserId);
        await _sut.CreateDeckAsync("Deck Usuario B", OtherUserId);
        await _sut.CreateDeckAsync("Otro Deck A", UserId);

        var decksA = await _sut.GetAllDecksAsync(UserId);
        var decksB = await _sut.GetAllDecksAsync(OtherUserId);

        Assert.Equal(2, decksA.Count);
        Assert.Single(decksB);
    }

    [Fact]
    public async Task GetDeckWithFilesAsync_NoDevuelveDeckDeOtroUsuario()
    {
        var deck = await _sut.CreateDeckAsync("Deck privado", UserId);

        var result = await _sut.GetDeckWithFilesAsync(deck.Id, OtherUserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteDeckAsync_NoPermiteBorrarDeckAjeno()
    {
        var deck = await _sut.CreateDeckAsync("Deck de otro", UserId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.DeleteDeckAsync(deck.Id, OtherUserId));
    }
}