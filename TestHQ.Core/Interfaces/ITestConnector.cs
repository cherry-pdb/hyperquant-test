using TestHQ.Core.Models;

namespace TestHQ.Core.Interfaces;

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


    event Action<Trade> NewBuyTrade;
    event Action<Trade> NewSellTrade;
    void SubscribeTrades(string pair, int maxCount = 100);
    void UnsubscribeTrades(string pair);

    event Action<Candle> CandleSeriesProcessing;
    void SubscribeCandles(
        string pair,
        int periodInSec,
        long? count,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null); // change the order of args because initialized args should be in the end
    void UnsubscribeCandles(string pair);

    #endregion

}