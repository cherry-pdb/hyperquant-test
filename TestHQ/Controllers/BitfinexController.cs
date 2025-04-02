using Microsoft.AspNetCore.Mvc;
using TestHQ.Core.Clients;

namespace TestHQ.Controllers;

// controller was created only for testing with swagger
[ApiController]
[Route("api/[controller]")]
public class BitfinexController : ControllerBase
{
    private readonly BitfinexRestClient _restClient;
    private readonly ILogger<BitfinexController> _logger;

    public BitfinexController(BitfinexRestClient restClient, ILogger<BitfinexController> logger)
    {
        _restClient = restClient;
        _logger = logger;
    }
    
    [HttpGet("trades/{pair}")]
    public async Task<IActionResult> GetTrades(string pair, int limit)
    {
        try
        {
            var trades = await _restClient.GetNewTradesAsync(pair, limit);
            
            return Ok(trades);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            
            return StatusCode(500);
        }
    }

    [HttpGet("candles/{pair}")]
    public async Task<IActionResult> GetCandles(
        string pair,
        int periodInSec,
        int? limit,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        try
        {
            var candles = await _restClient.GetCandleSeriesAsync(pair, periodInSec, limit, from, to);
            
            return Ok(candles);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            
            return StatusCode(500);
        }
    }
}