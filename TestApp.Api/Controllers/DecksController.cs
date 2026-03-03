using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestApp.Api.DTOs;
using TestApp.Core.Services;

namespace TestApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DecksController : ControllerBase
{
    private readonly IDeckService _deckService;

    public DecksController(IDeckService deckService)
    {
        _deckService = deckService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var decks = await _deckService.GetAllDecksAsync(UserId);
        return Ok(decks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var deck = await _deckService.GetDeckWithFilesAsync(id, UserId);
        if (deck == null)
            return NotFound();

        return Ok(deck);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("El nombre del mazo es requerido");

        var deck = await _deckService.CreateDeckAsync(request.Name, UserId);
        return CreatedAtAction(nameof(Get), new { id = deck.Id }, deck);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _deckService.DeleteDeckAsync(id, UserId);
        return NoContent();
    }
}
