using Microsoft.Extensions.Options;
using TestHQ.Core.Configurations;
using TestHQ.Core.Helpers;
using TestHQ.Core.Models;

namespace TestHQ.Core.Clients;

public class BitfinexRestClient
{
    private readonly HttpClient _httpClient;
    private readonly BitfinexConfiguration _configuration;

    public BitfinexRestClient(HttpClient httpClient, IOptions<BitfinexConfiguration> bitfinexOptions)
    {
        _httpClient = httpClient;
        _configuration = bitfinexOptions.Value;
    }

    #region API Methods

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        var url = new UrlBuilder(_configuration.BaseUrl)
            .AddPath("trades")
            .AddPath(pair)
            .AddPath("hist")
            .AddParameter("limit", maxCount.ToString())
            .Build();
        var response = await _httpClient.GetStringAsync(url);
        var trades = JsonConverter.ConvertTradesToCollection(pair, response);
        
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
            .AddParameter("start", from?.ToUnixTimeMilliseconds().ToString())
            .AddParameter("end", to?.ToUnixTimeMilliseconds().ToString())
            .Build();
        var response = await _httpClient.GetStringAsync(url);
        var candles = JsonConverter.ConvertCandlesToCollection(pair, response);
        
        return candles;
    }

    public async Task<Dictionary<string, decimal>> GetTickersAsync(string[] pairs)
    {
        var url = new UrlBuilder(_configuration.BaseUrl)
            .AddPath("tickers")
            .AddParameter("symbols", string.Join(",", pairs))
            .Build();
        var response = await _httpClient.GetStringAsync(url);
        var prices = JsonConverter.ConvertTickersToPricesCollection(response);

        return prices;
    }

    #endregion
}