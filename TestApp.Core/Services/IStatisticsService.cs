namespace TestApp.Core.Services;

/// <summary>
/// Estadísticas de un archivo
/// </summary>
public class FileStatistics
{
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int TotalAttempts { get; set; }
    public int CorrectAttempts { get; set; }
    public int IncorrectAttempts { get; set; }
    public double SuccessRate => TotalAttempts > 0 ? (double)CorrectAttempts / TotalAttempts * 100 : 0;
    public int QuestionsNeverAnswered { get; set; }
    public int QuestionsAlwaysCorrect { get; set; }
    public int QuestionsAlwaysWrong { get; set; }
    public List<QuestionStatistics> MostFailedQuestions { get; set; } = [];
}

/// <summary>
/// Estadísticas de un mazo
/// </summary>
public class DeckStatistics
{
    public int DeckId { get; set; }
    public string DeckName { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalAttempts { get; set; }
    public int CorrectAttempts { get; set; }
    public int IncorrectAttempts { get; set; }
    public double SuccessRate => TotalAttempts > 0 ? (double)CorrectAttempts / TotalAttempts * 100 : 0;
    public List<FileStatisticsSummary> FilesSummary { get; set; } = [];
}

/// <summary>
/// Resumen de estadísticas de un archivo (para mostrar en el mazo)
/// </summary>
public class FileStatisticsSummary
{
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int TotalAttempts { get; set; }
    public int CorrectAttempts { get; set; }
    public double SuccessRate => TotalAttempts > 0 ? (double)CorrectAttempts / TotalAttempts * 100 : 0;
}

/// <summary>
/// Estadísticas de una pregunta individual
/// </summary>
public class QuestionStatistics
{
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string Statement { get; set; } = string.Empty;
    public int TimesAnswered { get; set; }
    public int TimesCorrect { get; set; }
    public int TimesFailed { get; set; }
    public double SuccessRate => TimesAnswered > 0 ? (double)TimesCorrect / TimesAnswered * 100 : 0;
    public double FailureRate => TimesAnswered > 0 ? (double)TimesFailed / TimesAnswered * 100 : 0;
}

/// <summary>
/// Punto de datos para el grafico de progreso
/// </summary>
public class ProgressDataPoint
{
    public DateTime Date { get; set; }
    public int TotalAttempts { get; set; }
    public int CorrectAttempts { get; set; }
    public double SuccessRate => TotalAttempts > 0 ? (double)CorrectAttempts / TotalAttempts * 100 : 0;
    public string DateLabel => Date.ToString("dd/MM");
}

/// <summary>
/// Historial de progreso
/// </summary>
public class ProgressHistory
{
    public List<ProgressDataPoint> DailyProgress { get; set; } = [];
    public int TotalDaysStudied { get; set; }
    public DateTime? FirstStudyDate { get; set; }
    public DateTime? LastStudyDate { get; set; }
    public double OverallTrend { get; set; } // Positivo = mejorando, Negativo = empeorando
}

public interface IStatisticsService
{
    /// <summary>
    /// Obtiene las estadísticas de un archivo
    /// </summary>
    Task<FileStatistics> GetFileStatisticsAsync(int fileId);
    
    /// <summary>
    /// Obtiene las estadísticas de un mazo
    /// </summary>
    Task<DeckStatistics> GetDeckStatisticsAsync(int deckId);
    
    /// <summary>
    /// Obtiene las preguntas más falladas de un archivo
    /// </summary>
    Task<List<QuestionStatistics>> GetMostFailedQuestionsAsync(int fileId, int count = 5);
    
    /// <summary>
    /// Obtiene las estadísticas de una pregunta individual
    /// </summary>
    Task<QuestionStatistics> GetQuestionStatisticsAsync(int questionId);
    
    /// <summary>
    /// Obtiene el historial de progreso de un archivo
    /// </summary>
    Task<ProgressHistory> GetFileProgressHistoryAsync(int fileId, int days = 30);
    
    /// <summary>
    /// Obtiene el historial de progreso de un mazo
    /// </summary>
    Task<ProgressHistory> GetDeckProgressHistoryAsync(int deckId, int days = 30);
}
