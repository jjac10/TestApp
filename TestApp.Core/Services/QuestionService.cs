using Microsoft.EntityFrameworkCore;
using TestApp.Core.Data;
using TestApp.Core.DTOs;
using TestApp.Core.Models;

namespace TestApp.Core.Services;

public class QuestionService : IQuestionService
{
    private readonly AppDbContext _context;

    public QuestionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> ImportQuestionsAsync(int deckId, string fileName, List<QuestionImportDto> questions)
    {
        if (questions == null || questions.Count == 0)
        {
            throw new InvalidOperationException("El archivo no contiene preguntas válidas");
        }

        var questionFile = new QuestionFile
        {
            Name = fileName,
            DeckId = deckId,
            Questions = questions.Select(q => new Question
            {
                Number = q.Number,
                Statement = q.Statement,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer.FirstOrDefault(),
                Source = q.Source
            }).ToList()
        };

        _context.QuestionFiles.Add(questionFile);
        await _context.SaveChangesAsync();
        
        return questionFile.Questions.Count;
    }

    public async Task<List<Question>> GetQuestionsForExamAsync(int deckId, int count, QuestionFilter filter)
    {
        var query = _context.Questions
            .Include(q => q.Answers)
            .Where(q => q.File.DeckId == deckId);

        query = ApplyFilter(query, filter);

        // Cargar a memoria y luego ordenar aleatoriamente
        var questions = await query.ToListAsync();
        
        return questions
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .ToList();
    }

    public async Task<List<Question>> GetQuestionsFromFileAsync(int fileId, int count, QuestionFilter filter)
    {
        var query = _context.Questions
            .Include(q => q.Answers)
            .Where(q => q.FileId == fileId);

        query = ApplyFilter(query, filter);

        // Cargar a memoria y luego ordenar aleatoriamente
        var questions = await query.ToListAsync();
        
        return questions
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .ToList();
    }

    public async Task<List<Question>> GetAllQuestionsFromFileOrderedAsync(int fileId)
    {
        return await _context.Questions
            .Where(q => q.FileId == fileId)
            .OrderBy(q => q.Number)
            .ToListAsync();
    }

    public async Task RecordAnswerAsync(int questionId, char userAnswer)
    {
        var question = await _context.Questions.FindAsync(questionId)
            ?? throw new InvalidOperationException("Question not found");

        var answer = new Answer
        {
            QuestionId = questionId,
            UserAnswer = userAnswer,
            IsCorrect = userAnswer == question.CorrectAnswer
        };

        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFileAsync(int fileId)
    {
        // Usar SQL directo para eliminar en cascada de forma segura
        // Primero eliminar respuestas
        await _context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM Answers 
            WHERE QuestionId IN (
                SELECT Id FROM Questions WHERE FileId = {0}
            )", fileId);
        
        // Luego eliminar preguntas
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Questions WHERE FileId = {0}", fileId);
        
        // Finalmente eliminar el archivo
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM QuestionFiles WHERE Id = {0}", fileId);
    }

    private static IQueryable<Question> ApplyFilter(IQueryable<Question> query, QuestionFilter filter)
    {
        return filter switch
        {
            QuestionFilter.NeverAnswered => query.Where(q => !q.Answers.Any()),
            QuestionFilter.Failed => query.Where(q => q.Answers.Any(a => !a.IsCorrect)),
            _ => query
        };
    }

    public async Task UpdateCorrectAnswerAsync(int questionId, char newCorrectAnswer)
    {
        var question = await _context.Questions.FindAsync(questionId)
            ?? throw new InvalidOperationException("Question not found");
        
        question.CorrectAnswer = char.ToUpper(newCorrectAnswer);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountQuestionsInFileAsync(int fileId, QuestionFilter filter)
    {
        var query = _context.Questions
            .Include(q => q.Answers)
            .Where(q => q.FileId == fileId);

        query = ApplyFilter(query, filter);
        
        return await query.CountAsync();
    }

    public async Task<int> CountQuestionsInDeckAsync(int deckId, QuestionFilter filter)
    {
        var query = _context.Questions
            .Include(q => q.Answers)
            .Where(q => q.File.DeckId == deckId);

        query = ApplyFilter(query, filter);
        
        return await query.CountAsync();
    }
}