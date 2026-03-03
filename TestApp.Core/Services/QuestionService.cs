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
            throw new InvalidOperationException("El archivo no contiene preguntas v�lidas");
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

    public async Task<List<Question>> GetQuestionsForExamAsync(int deckId, int count, QuestionFilter filter, bool random = true, bool randomAnswers = false)
    {
        var query = _context.Questions
            .Include(q => q.Answers)
            .Where(q => q.File.DeckId == deckId);

        query = ApplyFilter(query, filter);

        var questions = await query.ToListAsync();

        var result = random
            ? questions.OrderBy(_ => Random.Shared.Next()).Take(count).ToList()
            : questions.OrderBy(q => q.Number).Take(count).ToList();

        if (randomAnswers)
            result.ForEach(ShuffleAnswers);

        return result;
    }

    public async Task<List<Question>> GetQuestionsFromFileAsync(int fileId, int count, QuestionFilter filter, bool random = true, bool randomAnswers = false)
    {
        var query = _context.Questions
            .Include(q => q.Answers)
            .Where(q => q.FileId == fileId);

        query = ApplyFilter(query, filter);

        var questions = await query.ToListAsync();

        var result = random
            ? questions.OrderBy(_ => Random.Shared.Next()).Take(count).ToList()
            : questions.OrderBy(q => q.Number).Take(count).ToList();

        if (randomAnswers)
            result.ForEach(ShuffleAnswers);

        return result;
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

    public async Task DeleteQuestionAsync(int questionId)
    {
        // Primero eliminar respuestas asociadas
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Answers WHERE QuestionId = {0}", questionId);
        
        // Luego eliminar la pregunta
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Questions WHERE Id = {0}", questionId);
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

    /// <summary>
    /// Mezcla las opciones A/B/C/D de una pregunta y actualiza CorrectAnswer para que siga apuntando a la opción correcta.
    /// </summary>
    private static void ShuffleAnswers(Question q)
    {
        var options = new List<(char Letter, string Text)>
        {
            ('A', q.OptionA),
            ('B', q.OptionB),
            ('C', q.OptionC),
            ('D', q.OptionD)
        };

        // Fisher-Yates shuffle
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }

        // Find where the correct answer ended up
        var correctText = q.CorrectAnswer switch
        {
            'A' => q.OptionA,
            'B' => q.OptionB,
            'C' => q.OptionC,
            'D' => q.OptionD,
            _ => q.OptionA
        };

        q.OptionA = options[0].Text;
        q.OptionB = options[1].Text;
        q.OptionC = options[2].Text;
        q.OptionD = options[3].Text;

        // Update correct answer to new position
        char[] letters = { 'A', 'B', 'C', 'D' };
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].Text == correctText)
            {
                q.CorrectAnswer = letters[i];
                break;
            }
        }
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