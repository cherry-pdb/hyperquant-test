using System.Text.Json;
using Microsoft.Extensions.Options;
using TestHQ.Core.Configurations;
using TestHQ.Core.Helpers;
using TestHQ.Core.Interfaces;
using TestHQ.Core.Models;

namespace TestHQ.Core.Clients;

public class BitfinexRestClient : ITestConnector
{
    private readonly HttpClient _httpClient;
    private readonly BitfinexConfiguration _configuration;

    public BitfinexRestClient(HttpClient httpClient, IOptions<BitfinexConfiguration> bitfinexOptions)
    {
        _httpClient = httpClient;
        _configuration = bitfinexOptions.Value;
    }

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        var url = $"{_configuration.BaseUrl}/trades/{pair}/hist?limit={maxCount}";
        var response = await _httpClient.GetStringAsync(url);
        var trades = JsonSerializer.Deserialize<IEnumerable<Trade>>(response) ?? [];
        
        return trades;
    }

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, long? count, DateTimeOffset? from, DateTimeOffset? to = null)
    {
        var periodForUrl = PeriodConverter.GetPeriodString(periodInSec);
        var url = new UrlBuilder(_configuration.BaseUrl)
            .AddPath("candles")
            .AddPath($"trade:{periodForUrl}:{pair}")
            .AddPath("hist")
            .AddParameter("limit", count.ToString())
            .Build();
        var response = await _httpClient.GetStringAsync(url);
        var candles = JsonSerializer.Deserialize<IEnumerable<Candle>>(response) ?? [];
        
        return candles;
    }

    public event Action<Trade>? NewBuyTrade;
    public event Action<Trade>? NewSellTrade;
    public void SubscribeTrades(string pair, int maxCount = 100) {}

    public void UnsubscribeTrades(string pair) {}

    public event Action<Candle>? CandleSeriesProcessing;

    public void SubscribeCandles(string pair, int periodInSec, long? count, DateTimeOffset? from = null, DateTimeOffset? to = null) {}

    public void UnsubscribeCandles(string pair) {}
}