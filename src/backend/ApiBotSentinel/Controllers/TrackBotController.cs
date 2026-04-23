using ApiBotSentinel.Dts;
using ApiBotSentinel.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBotSentinel.Controllers;

[Route("api/[controller]/[action]")]
public class TrackBotController : ControllerBase
{
    private readonly ITrackBotService _trackBotService;

    public TrackBotController(ITrackBotService trackBotService)
    {
        _trackBotService = trackBotService;
    }

    [HttpPost]
    public async Task<IActionResult> Track([FromBody] TrackBotPostDtq trackBotPostQuery)
    {
        if (!CheckHeaderApiKey())
            return Unauthorized();

        await _trackBotService.TrackAsync(trackBotPostQuery);
        return Ok();
    }

    private bool CheckHeaderApiKey()
    {
        if (Request.Headers["trackbot-api-key"] != "vT9fK2xQ8LmR4Zp7Yw3NcD1Hs6JbA0Ue")
            return false;
        return true;
    }
}