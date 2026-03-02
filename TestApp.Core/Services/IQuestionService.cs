using TestApp.Core.DTOs;
using TestApp.Core.Models;

namespace TestApp.Core.Services;

public enum QuestionFilter
{
    All,
    NeverAnswered,
    Failed
}

public interface IQuestionService
{
    Task<int> ImportQuestionsAsync(int deckId, string fileName, List<QuestionImportDto> questions);
    Task<List<Question>> GetQuestionsForExamAsync(int deckId, int count, QuestionFilter filter);
    Task<List<Question>> GetQuestionsFromFileAsync(int fileId, int count, QuestionFilter filter);
    Task<List<Question>> GetAllQuestionsFromFileOrderedAsync(int fileId);
    Task RecordAnswerAsync(int questionId, char userAnswer);
    Task DeleteFileAsync(int fileId);
    Task UpdateCorrectAnswerAsync(int questionId, char newCorrectAnswer);
    
    /// <summary>
    /// Elimina una pregunta y sus respuestas asociadas
    /// </summary>
    Task DeleteQuestionAsync(int questionId);
    
    /// <summary>
    /// Cuenta las preguntas de un archivo según el filtro
    /// </summary>
    Task<int> CountQuestionsInFileAsync(int fileId, QuestionFilter filter);
    
    /// <summary>
    /// Cuenta las preguntas de un mazo según el filtro
    /// </summary>
    Task<int> CountQuestionsInDeckAsync(int deckId, QuestionFilter filter);
}