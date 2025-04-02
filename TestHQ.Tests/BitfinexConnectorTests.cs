using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestHQ.Core.Clients;
using TestHQ.Core.Configurations;
using TestHQ.Core.Connector;
using TestHQ.Core.Models;

namespace TestHQ.Tests;

[TestFixture]
public class BitfinexConnectorTests
{
    private ITestConnector _bitfinexConnector;
    
    private const string TestPair = "tBTCUSD";

    [SetUp]
    public void SetUp()
    {
        var bitfinexOptions = Options.Create(new BitfinexConfiguration
        {
            BaseUrl = "https://api-pub.bitfinex.com/v2",
            WsUrl = "wss://api.bitfinex.com/ws/2"
        });
        var loggerWebSocket = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BitfinexWebSocketClient>();
        var bitfinexWebSocketClient = new BitfinexWebSocketClient(bitfinexOptions, loggerWebSocket);
        var bitfinexRestClient = new BitfinexRestClient(new HttpClient(), bitfinexOptions);

        _bitfinexConnector = new BitfinexConnector(bitfinexRestClient, bitfinexWebSocketClient);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _bitfinexConnector.StopAsync();
    }

    #region REST Tests

    [Test]
    public async Task ShouldReturnTrades()
    {
        // Arrange
        const string pair = "tBTCUSD";
        const int maxCount = 5;
        
        // Act
        var trades = await _bitfinexConnector.GetNewTradesAsync(pair, maxCount);
        
        // Assert
        Assert.That(trades, Is.Not.Null);
        Assert.That(trades, Is.Not.Empty);
        Assert.That(trades, Has.All.Matches<Trade>(trade => trade.Pair == pair));
    }
    
    [Test]
    public async Task ShouldReturnCandles()
    {
        // Arrange
        const string pair = "tBTCUSD";
        const int periodInSec = 60;
        const long count = 5;
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow;
        
        // Act
        var candles = await _bitfinexConnector.GetCandleSeriesAsync(pair, periodInSec, count, from, to);
        
        // Assert
        Assert.That(candles, Is.Not.Null);
        Assert.That(candles, Is.Not.Empty);
        Assert.That(candles, Has.All.Matches<Candle>(candle => candle.Pair == pair));

    }

    #endregion

    #region WebSocket Tests

    [Test]
    public async Task ShouldSubscribeTrades()
    {
        // Arrange
        var tradeReceived = false;
        _bitfinexConnector.NewBuyTrade += trade =>
        {
            if (trade.Side == "buy")
            {
                tradeReceived = true;
            }
        };
        _bitfinexConnector.NewSellTrade += trade =>
        {
            if (trade.Side == "sell")
            {
                tradeReceived = true;
            }
        };
        
        // Act
        await _bitfinexConnector.StartAsync();
        await _bitfinexConnector.SubscribeTradesAsync(TestPair);
        
        await Task.Delay(5000); // time for connecting by ws and processing messages
        
        // Assert
        Assert.That(tradeReceived, Is.EqualTo(true));
        await _bitfinexConnector.StopAsync();
    }

    [Test]
    public async Task ShouldSubscribeCandles()
    {
        // Arrange
        var candleReceived = false;
        _bitfinexConnector.CandleSeriesProcessing += _ => candleReceived = true;

        // Act
        await _bitfinexConnector.StartAsync();
        await _bitfinexConnector.SubscribeCandlesAsync(TestPair, 60, 10);

        await Task.Delay(5000);

        // Assert
        Assert.That(candleReceived, Is.EqualTo(true));
        await _bitfinexConnector.StopAsync();
    }

    [Test]
    public async Task ShouldUnsubscribeTrades()
    {
        // Arrange
        await _bitfinexConnector.StartAsync();
        await _bitfinexConnector.SubscribeTradesAsync(TestPair);
        
        await Task.Delay(1000); // time to start ws connection and subscribing
        
        // Act
        await _bitfinexConnector.UnsubscribeTradesAsync(TestPair);

        // Assert
        var tradeReceivedAfterUnsubscribe = false;
        _bitfinexConnector.NewBuyTrade += _ => tradeReceivedAfterUnsubscribe = true;
        _bitfinexConnector.NewSellTrade += _ => tradeReceivedAfterUnsubscribe = true;

        await Task.Delay(5000); // wait 2 seconds to make sure that the messages aren't receiving
        Assert.That(tradeReceivedAfterUnsubscribe, Is.EqualTo(false));
        await _bitfinexConnector.StopAsync();
    }

    [Test]
    public async Task ShouldUnsubscribeCandles()
    {
        // Arrange
        await _bitfinexConnector.StartAsync();
        await _bitfinexConnector.SubscribeCandlesAsync(TestPair, 60, 100);

        await Task.Delay(1000); // time to start ws connection and subscribing

        // Act
        await _bitfinexConnector.UnsubscribeCandlesAsync(TestPair);

        // Assert
        var candleReceivedAfterUnsubscribe = false;
        _bitfinexConnector.CandleSeriesProcessing += _ => candleReceivedAfterUnsubscribe = true;

        await Task.Delay(2000); // wait 2 seconds to make sure that the messages aren't receiving
        Assert.That(candleReceivedAfterUnsubscribe, Is.EqualTo(false));
        await _bitfinexConnector.StopAsync();
    }

    #endregion
}