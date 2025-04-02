using TestHQ.Core.Clients;
using TestHQ.Core.Models;

namespace TestHQ.Core.Connector;

public class BitfinexConnector : ITestConnector
{
    private readonly BitfinexRestClient _restClient;
    private readonly BitfinexWebSocketClient _webSocketClient;

    public BitfinexConnector(BitfinexRestClient restClient, BitfinexWebSocketClient webSocketClient)
    {
        _restClient = restClient;
        _webSocketClient = webSocketClient;

        _webSocketClient.NewBuyTrade += trade => NewBuyTrade?.Invoke(trade);
        _webSocketClient.NewSellTrade += trade => NewSellTrade?.Invoke(trade);
        _webSocketClient.CandleSeriesProcessing += candle => CandleSeriesProcessing?.Invoke(candle);
    }

    #region REST

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        return await _restClient.GetNewTradesAsync(pair, maxCount);
    }

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(
        string pair,
        int periodInSec,
        long? count,
        DateTimeOffset? from,
        DateTimeOffset? to = null)
    {
        return await _restClient.GetCandleSeriesAsync(pair, periodInSec, count, from, to);
    }

    public async Task<Dictionary<string, decimal>> GetTickersAsync(string[] pairs)
    {
        return await _restClient.GetTickersAsync(pairs);
    }

    #endregion

    #region WebSocket

    public event Action<Trade> NewBuyTrade;
    public event Action<Trade> NewSellTrade;
    public event Action<Candle> CandleSeriesProcessing;

    public async Task StartAsync()
    {
        await _webSocketClient.StartAsync();
    }
    
    public async Task StopAsync()
    {
        await _webSocketClient.StopAsync();
    }

    public async Task SubscribeTradesAsync(string pair, int maxCount = 100)
    {
        await _webSocketClient.SubscribeTradesAsync(pair, maxCount);
    }

    public async Task UnsubscribeTradesAsync(string pair)
    {
        await _webSocketClient.UnsubscribeTradesAsync(pair);
    }

    public async Task SubscribeCandlesAsync(
        string pair,
        int periodInSec,
        long? count,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null)
    {
        await _webSocketClient.SubscribeCandlesAsync(pair, periodInSec, count, from, to);
    }

    public async Task UnsubscribeCandlesAsync(string pair)
    {
        await _webSocketClient.UnsubscribeCandlesAsync(pair);
    }
    
    #endregion
}
