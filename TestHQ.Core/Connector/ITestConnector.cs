using TestHQ.Core.Models;

namespace TestHQ.Core.Connector;

public interface ITestConnector
{
    #region Rest

    Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);
    Task<IEnumerable<Candle>> GetCandleSeriesAsync(
        string pair,
        int periodInSec,
        long? count,
        DateTimeOffset? from,
        DateTimeOffset? to = null); // change the order of args because initialized args should be in the end

    #endregion

    #region Socket

    Task StartAsync(); // add this method for web socket connecting
    Task StopAsync(); // add this method for closing web socket connection 
    event Action<Trade> NewBuyTrade;
    event Action<Trade> NewSellTrade;
    Task SubscribeTradesAsync(string pair, int maxCount = 100);
    Task UnsubscribeTradesAsync(string pair);

    event Action<Candle> CandleSeriesProcessing;
    Task SubscribeCandlesAsync(
        string pair,
        int periodInSec,
        long? count,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null); // change the order of args because initialized args should be in the end
    Task UnsubscribeCandlesAsync(string pair);

    #endregion

}