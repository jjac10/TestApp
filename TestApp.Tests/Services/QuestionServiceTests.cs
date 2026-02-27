using TestApp.Core.DTOs;
using TestApp.Core.Models;
using TestApp.Core.Services;
using TestApp.Tests.Helpers;

namespace TestApp.Tests.Services;

public class QuestionServiceTests : IDisposable
{
    private const string UserId = TestDbContextFactory.TestUserId;
    private readonly TestApp.Core.Data.AppDbContext _context;
    private readonly QuestionService _sut;
    private readonly DeckService _deckService;

    public QuestionServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _sut = new QuestionService(_context);
        _deckService = new DeckService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private List<QuestionImportDto> CreateSampleQuestions(int count = 3)
    {
        return Enumerable.Range(1, count).Select(i => new QuestionImportDto
        {
            Number = i,
            Statement = $"Pregunta {i}",
            OptionA = "Opción A",
            OptionB = "Opción B",
            OptionC = "Opción C",
            OptionD = "Opción D",
            CorrectAnswer = "A",
            Source = "Test"
        }).ToList();
    }

    // --- ImportQuestionsAsync ---

    [Fact]
    public async Task ImportQuestionsAsync_ImportaCorrectamente()
    {
        var deck = await _deckService.CreateDeckAsync("Deck Test", UserId);
        var questions = CreateSampleQuestions(5);

        var count = await _sut.ImportQuestionsAsync(deck.Id, "Tema 1", questions);

        Assert.Equal(5, count);
    }

    [Fact]
    public async Task ImportQuestionsAsync_ListaVacia_LanzaExcepcion()
    {
        var deck = await _deckService.CreateDeckAsync("Deck Test", UserId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportQuestionsAsync(deck.Id, "Vacío", []));
    }

    [Fact]
    public async Task ImportQuestionsAsync_Null_LanzaExcepcion()
    {
        var deck = await _deckService.CreateDeckAsync("Deck Test", UserId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ImportQuestionsAsync(deck.Id, "Null", null!));
    }

    // --- GetAllQuestionsFromFileOrderedAsync ---

    [Fact]
    public async Task GetAllQuestionsFromFileOrderedAsync_DevuelveOrdenadas()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        var dtos = new List<QuestionImportDto>
        {
            new() { Number = 3, Statement = "Tercera", OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D", CorrectAnswer = "A" },
            new() { Number = 1, Statement = "Primera", OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D", CorrectAnswer = "B" },
            new() { Number = 2, Statement = "Segunda", OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D", CorrectAnswer = "C" },
        };
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", dtos);

        var file = _context.QuestionFiles.First();
        var questions = await _sut.GetAllQuestionsFromFileOrderedAsync(file.Id);

        Assert.Equal(3, questions.Count);
        Assert.Equal(1, questions[0].Number);
        Assert.Equal(2, questions[1].Number);
        Assert.Equal(3, questions[2].Number);
    }

    // --- RecordAnswerAsync ---

    [Fact]
    public async Task RecordAnswerAsync_RespuestaCorrecta_RegistraIsCorrectTrue()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(1));
        var question = _context.Questions.First();

        await _sut.RecordAnswerAsync(question.Id, 'A');

        var answer = _context.Answers.First();
        Assert.True(answer.IsCorrect);
        Assert.Equal('A', answer.UserAnswer);
    }

    [Fact]
    public async Task RecordAnswerAsync_RespuestaIncorrecta_RegistraIsCorrectFalse()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(1));
        var question = _context.Questions.First();

        await _sut.RecordAnswerAsync(question.Id, 'C');

        var answer = _context.Answers.First();
        Assert.False(answer.IsCorrect);
    }

    [Fact]
    public async Task RecordAnswerAsync_PreguntaInexistente_LanzaExcepcion()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.RecordAnswerAsync(999, 'A'));
    }

    // --- UpdateCorrectAnswerAsync ---

    [Fact]
    public async Task UpdateCorrectAnswerAsync_ActualizaRespuesta()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(1));
        var question = _context.Questions.First();
        Assert.Equal('A', question.CorrectAnswer);

        await _sut.UpdateCorrectAnswerAsync(question.Id, 'd');

        var updated = _context.Questions.First();
        Assert.Equal('D', updated.CorrectAnswer);
    }

    [Fact]
    public async Task UpdateCorrectAnswerAsync_PreguntaInexistente_LanzaExcepcion()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UpdateCorrectAnswerAsync(999, 'B'));
    }

    // --- Filtros ---

    [Fact]
    public async Task GetQuestionsForExamAsync_FiltroAll_DevuelveTodas()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(5));

        var questions = await _sut.GetQuestionsForExamAsync(deck.Id, 10, QuestionFilter.All);

        Assert.Equal(5, questions.Count);
    }

    [Fact]
    public async Task GetQuestionsForExamAsync_FiltroNeverAnswered_ExcluyeRespondidas()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(3));
        var firstQuestion = _context.Questions.First();
        await _sut.RecordAnswerAsync(firstQuestion.Id, 'A');

        var questions = await _sut.GetQuestionsForExamAsync(deck.Id, 10, QuestionFilter.NeverAnswered);

        Assert.Equal(2, questions.Count);
        Assert.DoesNotContain(questions, q => q.Id == firstQuestion.Id);
    }

    [Fact]
    public async Task GetQuestionsForExamAsync_FiltroFailed_SoloFalladas()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(3));
        var questions = _context.Questions.ToList();
        // Responder la primera correctamente y la segunda incorrectamente
        await _sut.RecordAnswerAsync(questions[0].Id, 'A'); // Correcta
        await _sut.RecordAnswerAsync(questions[1].Id, 'C'); // Incorrecta

        var failed = await _sut.GetQuestionsForExamAsync(deck.Id, 10, QuestionFilter.Failed);

        Assert.Single(failed);
        Assert.Equal(questions[1].Id, failed[0].Id);
    }

    [Fact]
    public async Task GetQuestionsForExamAsync_LimitaCantidad()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(10));

        var questions = await _sut.GetQuestionsForExamAsync(deck.Id, 3, QuestionFilter.All);

        Assert.Equal(3, questions.Count);
    }

    // --- CountQuestions ---

    [Fact]
    public async Task CountQuestionsInFileAsync_CuentaCorrectamente()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(7));
        var file = _context.QuestionFiles.First();

        var count = await _sut.CountQuestionsInFileAsync(file.Id, QuestionFilter.All);

        Assert.Equal(7, count);
    }

    [Fact]
    public async Task CountQuestionsInDeckAsync_CuentaTodasLasPreguntas()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema 1", CreateSampleQuestions(3));
        await _sut.ImportQuestionsAsync(deck.Id, "Tema 2", CreateSampleQuestions(4));

        var count = await _sut.CountQuestionsInDeckAsync(deck.Id, QuestionFilter.All);

        Assert.Equal(7, count);
    }

    // --- DeleteFileAsync ---

    [Fact]
    public async Task DeleteFileAsync_EliminaArchivoYPreguntas()
    {
        var deck = await _deckService.CreateDeckAsync("Deck", UserId);
        await _sut.ImportQuestionsAsync(deck.Id, "Tema", CreateSampleQuestions(3));
        var file = _context.QuestionFiles.First();
        // Registrar una respuesta para verificar que también se borra
        var question = _context.Questions.First();
        await _sut.RecordAnswerAsync(question.Id, 'A');

        await _sut.DeleteFileAsync(file.Id);

        Assert.Empty(_context.QuestionFiles);
        Assert.Empty(_context.Questions);
        Assert.Empty(_context.Answers);
    }
}