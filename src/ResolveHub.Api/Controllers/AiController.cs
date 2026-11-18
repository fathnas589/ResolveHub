namespace ResolveHub.Api.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResolveHub.Core.DTOs.Ai;
using ResolveHub.Services.Interfaces;

/// <summary>AI assistance endpoints (Claude). All suggestions are advisory — users confirm before submitting.</summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiTriageService _triageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiTriageService triageService, ICurrentUserService currentUserService, ILogger<AiController> logger)
    {
        _triageService = triageService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>Lets the UI decide whether to show AI features.</summary>
    [HttpGet("status")]
    public IActionResult Status() => Ok(new { enabled = _triageService.IsEnabled });

    /// <summary>Suggests department, category, subcategory, priority and type for a ticket being raised.</summary>
    [HttpPost("triage-suggestion")]
    [ProducesResponseType(typeof(TriageSuggestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SuggestTriage([FromBody] TriageSuggestionRequestDto request, CancellationToken cancellationToken)
    {
        if (!_triageService.IsEnabled)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "AI suggestions are not configured." });
        }

        try
        {
            var suggestion = await _triageService.SuggestAsync(_currentUserService.CompanyId, request, cancellationToken);
            return Ok(suggestion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (AnthropicRateLimitException)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "AI is busy right now. Please try again in a moment." });
        }
        catch (AnthropicApiException ex)
        {
            _logger.LogError(ex, "Claude API error during triage suggestion");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "AI suggestion is unavailable right now. Please fill the form manually." });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error during triage suggestion");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "AI suggestion is unavailable right now. Please fill the form manually." });
        }
    }
}
