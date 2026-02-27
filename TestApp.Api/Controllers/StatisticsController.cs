using Microsoft.AspNetCore.Mvc;
using TestApp.Core.Services;

namespace TestApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Obtiene estadísticas de un archivo
    /// </summary>
    [HttpGet("file/{fileId}")]
    public async Task<IActionResult> GetFileStatistics(int fileId)
    {
        var stats = await _statisticsService.GetFileStatisticsAsync(fileId);
        return Ok(stats);
    }

    /// <summary>
    /// Obtiene estadísticas de un mazo
    /// </summary>
    [HttpGet("deck/{deckId}")]
    public async Task<IActionResult> GetDeckStatistics(int deckId)
    {
        var stats = await _statisticsService.GetDeckStatisticsAsync(deckId);
        return Ok(stats);
    }

    /// <summary>
    /// Obtiene las preguntas más falladas de un archivo
    /// </summary>
    [HttpGet("file/{fileId}/most-failed")]
    public async Task<IActionResult> GetMostFailedQuestions(int fileId, [FromQuery] int count = 5)
    {
        var questions = await _statisticsService.GetMostFailedQuestionsAsync(fileId, count);
        return Ok(questions);
    }

    /// <summary>
    /// Obtiene estadísticas de una pregunta individual
    /// </summary>
    [HttpGet("question/{questionId}")]
    public async Task<IActionResult> GetQuestionStatistics(int questionId)
    {
        var stats = await _statisticsService.GetQuestionStatisticsAsync(questionId);
        return Ok(stats);
    }

    /// <summary>
    /// Obtiene el historial de progreso de un archivo
    /// </summary>
    [HttpGet("file/{fileId}/progress")]
    public async Task<IActionResult> GetFileProgress(int fileId, [FromQuery] int days = 30)
    {
        var history = await _statisticsService.GetFileProgressHistoryAsync(fileId, days);
        return Ok(history);
    }

    /// <summary>
    /// Obtiene el historial de progreso de un mazo
    /// </summary>
    [HttpGet("deck/{deckId}/progress")]
    public async Task<IActionResult> GetDeckProgress(int deckId, [FromQuery] int days = 30)
    {
        var history = await _statisticsService.GetDeckProgressHistoryAsync(deckId, days);
        return Ok(history);
    }
}
