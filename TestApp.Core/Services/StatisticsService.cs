using Microsoft.EntityFrameworkCore;
using TestApp.Core.Data;

namespace TestApp.Core.Services;

public class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _context;

    public StatisticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FileStatistics> GetFileStatisticsAsync(int fileId)
    {
        var file = await _context.QuestionFiles
            .Include(f => f.Questions)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(f => f.Id == fileId);

        if (file == null)
        {
            return new FileStatistics();
        }

        var allAnswers = file.Questions.SelectMany(q => q.Answers).ToList();
        var questionsWithAnswers = file.Questions.Where(q => q.Answers.Any()).ToList();

        var stats = new FileStatistics
        {
            FileId = file.Id,
            FileName = file.Name,
            TotalQuestions = file.Questions.Count,
            TotalAttempts = allAnswers.Count,
            CorrectAttempts = allAnswers.Count(a => a.IsCorrect),
            IncorrectAttempts = allAnswers.Count(a => !a.IsCorrect),
            QuestionsNeverAnswered = file.Questions.Count(q => !q.Answers.Any()),
            QuestionsAlwaysCorrect = questionsWithAnswers.Count(q => q.Answers.All(a => a.IsCorrect)),
            QuestionsAlwaysWrong = questionsWithAnswers.Count(q => q.Answers.All(a => !a.IsCorrect)),
            MostFailedQuestions = await GetMostFailedQuestionsAsync(fileId, 5)
        };

        return stats;
    }

    public async Task<DeckStatistics> GetDeckStatisticsAsync(int deckId)
    {
        var deck = await _context.Decks
            .Include(d => d.Files)
                .ThenInclude(f => f.Questions)
                    .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        if (deck == null)
        {
            return new DeckStatistics();
        }

        var allQuestions = deck.Files.SelectMany(f => f.Questions).ToList();
        var allAnswers = allQuestions.SelectMany(q => q.Answers).ToList();

        var filesSummary = deck.Files.Select(f =>
        {
            var fileAnswers = f.Questions.SelectMany(q => q.Answers).ToList();
            return new FileStatisticsSummary
            {
                FileId = f.Id,
                FileName = f.Name,
                TotalQuestions = f.Questions.Count,
                TotalAttempts = fileAnswers.Count,
                CorrectAttempts = fileAnswers.Count(a => a.IsCorrect)
            };
        }).OrderByDescending(f => f.TotalAttempts).ToList();

        var stats = new DeckStatistics
        {
            DeckId = deck.Id,
            DeckName = deck.Name,
            TotalFiles = deck.Files.Count,
            TotalQuestions = allQuestions.Count,
            TotalAttempts = allAnswers.Count,
            CorrectAttempts = allAnswers.Count(a => a.IsCorrect),
            IncorrectAttempts = allAnswers.Count(a => !a.IsCorrect),
            FilesSummary = filesSummary
        };

        return stats;
    }

    public async Task<List<QuestionStatistics>> GetMostFailedQuestionsAsync(int fileId, int count = 5)
    {
        var questions = await _context.Questions
            .Include(q => q.Answers)
            .Where(q => q.FileId == fileId)
            .ToListAsync();

        var failedQuestions = questions
            .Where(q => q.Answers.Any(a => !a.IsCorrect))
            .Select(q => new QuestionStatistics
            {
                QuestionId = q.Id,
                QuestionNumber = q.Number,
                Statement = q.Statement.Length > 100 ? q.Statement[..100] + "..." : q.Statement,
                TimesAnswered = q.Answers.Count,
                TimesCorrect = q.Answers.Count(a => a.IsCorrect),
                TimesFailed = q.Answers.Count(a => !a.IsCorrect)
            })
            .OrderByDescending(q => q.FailureRate)
            .ThenByDescending(q => q.TimesFailed)
            .Take(count)
            .ToList();

        return failedQuestions;
    }

    public async Task<QuestionStatistics> GetQuestionStatisticsAsync(int questionId)
    {
        var question = await _context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null)
        {
            return new QuestionStatistics();
        }

        return new QuestionStatistics
        {
            QuestionId = question.Id,
            QuestionNumber = question.Number,
            Statement = question.Statement,
            TimesAnswered = question.Answers.Count,
            TimesCorrect = question.Answers.Count(a => a.IsCorrect),
            TimesFailed = question.Answers.Count(a => !a.IsCorrect)
        };
    }

    public async Task<ProgressHistory> GetFileProgressHistoryAsync(int fileId, int days = 30)
    {
        var startDate = DateTime.Now.Date.AddDays(-days + 1);
        
        var answers = await _context.Answers
            .Include(a => a.Question)
            .Where(a => a.Question.FileId == fileId && a.AnsweredAt >= startDate)
            .ToListAsync();

        return BuildProgressHistory(answers, days);
    }

    public async Task<ProgressHistory> GetDeckProgressHistoryAsync(int deckId, int days = 30)
    {
        var startDate = DateTime.Now.Date.AddDays(-days + 1);
        
        var fileIds = await _context.QuestionFiles
            .Where(f => f.DeckId == deckId)
            .Select(f => f.Id)
            .ToListAsync();

        var answers = await _context.Answers
            .Include(a => a.Question)
            .Where(a => fileIds.Contains(a.Question.FileId) && a.AnsweredAt >= startDate)
            .ToListAsync();

        return BuildProgressHistory(answers, days);
    }

    private static ProgressHistory BuildProgressHistory(List<Models.Answer> answers, int days)
    {
        var startDate = DateTime.Now.Date.AddDays(-days + 1);
        var history = new ProgressHistory();

        if (!answers.Any())
        {
            // Devolver lista vacía de días
            for (int i = 0; i < days; i++)
            {
                history.DailyProgress.Add(new ProgressDataPoint
                {
                    Date = startDate.AddDays(i),
                    TotalAttempts = 0,
                    CorrectAttempts = 0
                });
            }
            return history;
        }

        // Agrupar por día
        var groupedByDay = answers
            .GroupBy(a => a.AnsweredAt.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Crear puntos de datos para cada día
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var dayAnswers = groupedByDay.GetValueOrDefault(date, []);
            
            history.DailyProgress.Add(new ProgressDataPoint
            {
                Date = date,
                TotalAttempts = dayAnswers.Count,
                CorrectAttempts = dayAnswers.Count(a => a.IsCorrect)
            });
        }

        // Calcular métricas generales
        var allAnswerDates = answers.Select(a => a.AnsweredAt).ToList();
        history.TotalDaysStudied = groupedByDay.Count;
        history.FirstStudyDate = allAnswerDates.Min();
        history.LastStudyDate = allAnswerDates.Max();

        // Calcular tendencia (comparar primera mitad vs segunda mitad del período)
        var midPoint = days / 2;
        var firstHalf = history.DailyProgress.Take(midPoint).Where(d => d.TotalAttempts > 0).ToList();
        var secondHalf = history.DailyProgress.Skip(midPoint).Where(d => d.TotalAttempts > 0).ToList();

        if (firstHalf.Any() && secondHalf.Any())
        {
            var firstHalfAvg = firstHalf.Average(d => d.SuccessRate);
            var secondHalfAvg = secondHalf.Average(d => d.SuccessRate);
            history.OverallTrend = secondHalfAvg - firstHalfAvg;
        }

        return history;
    }
}
