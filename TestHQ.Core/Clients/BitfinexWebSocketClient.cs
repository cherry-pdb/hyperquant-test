using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestHQ.Core.Configurations;
using TestHQ.Core.Helpers;
using TestHQ.Core.Models;

namespace TestHQ.Core.Clients;

public class BitfinexWebSocketClient
{
    private readonly ILogger<BitfinexWebSocketClient> _logger;
    private readonly CancellationTokenSource _cts;
    private readonly ClientWebSocket _webSocket;
    private readonly Dictionary<string, int> _candleChannels;
    private readonly Dictionary<string, int> _tradeChannels;
    private readonly Uri _bitfinexUri;
    private string _currentPair;
    
    private readonly List<string> _messageTypes = ["tu", "te", "fte", "ftu" ];

    public BitfinexWebSocketClient(
        IOptions<BitfinexConfiguration> bitfinexOptions,
        ILogger<BitfinexWebSocketClient> logger,
        ClientWebSocket? clientWebSocket = null)
    {
        _logger = logger;
        _cts = new CancellationTokenSource();
        _webSocket = clientWebSocket ?? new ClientWebSocket();
        _candleChannels = new Dictionary<string, int>();
        _tradeChannels = new Dictionary<string, int>();
        _bitfinexUri = new Uri(bitfinexOptions.Value.WsUrl);
        _currentPair = string.Empty;
    }

    public event Action<Trade> NewBuyTrade;
    public event Action<Trade> NewSellTrade;
    public event Action<Candle> CandleSeriesProcessing;

    public async Task StartAsync()
    {
        _logger.LogInformation("Connection...");
        await _webSocket.ConnectAsync(_bitfinexUri, _cts.Token);
        _logger.LogInformation("WebSocket connection was started.");
        _ = ReceiveMessagesAsync();
    }

    public async Task StopAsync()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"Stop by method {nameof(StopAsync)}", _cts.Token);
            _logger.LogInformation("WebSocket connection was closed.");
        }
        
        _webSocket.Dispose();
    }

    public async Task SubscribeTradesAsync(string pair, int maxCount = 100)
    {
        var message = JsonSerializer.Serialize(new { @event = "subscribe", channel = "trades", symbol = pair });
        await _webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, _cts.Token);
        _currentPair = pair;
        _logger.LogInformation("Subscribed to trades of pair {Pair}.", pair);
    }

    public async Task UnsubscribeTradesAsync(string pair)
    {
        if (!_tradeChannels.TryGetValue(pair, out var channelId))
            throw new InvalidOperationException($"There isn't channel for pair {pair}.");

        var message = JsonSerializer.Serialize(new { @event = "unsubscribe", chanId = channelId });
        await _webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, _cts.Token);
        _tradeChannels.Remove(pair);
        _currentPair = string.Empty;
        _logger.LogInformation("Unsubscribed from trades of pair {Pair}.", pair);
    }

    public async Task SubscribeCandlesAsync(
        string pair,
        int periodInSec,
        long? count,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null)
    {
        var period = PeriodConverter.GetPeriodString(periodInSec);
        var keyValue = $"trade:{period}:{pair}";
        var message = JsonSerializer.Serialize(new { @event = "subscribe", channel = "candles", key = keyValue });
        await _webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, _cts.Token);
        _currentPair = pair;
        _logger.LogInformation("Subscribed to candles of pair {Pair}.", pair);
    }

    public async Task UnsubscribeCandlesAsync(string pair)
    {
        if (!_candleChannels.TryGetValue(pair, out var channelId))
            throw new InvalidOperationException($"There isn't channel for pair {pair}.");
            
        var message = JsonSerializer.Serialize(new { @event = "unsubscribe", chanId = channelId });
        await _webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, _cts.Token);
        _candleChannels.Remove(pair);
        _currentPair = string.Empty;
        _logger.LogInformation("Unsubscribed from candles of pair {Pair}.", pair);
    }
    
    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[32768];
        
        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(buffer, _cts.Token);
            
            if (result.MessageType == WebSocketMessageType.Close)
                break;
            
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            ProcessMessage(message);
        }
    }

    private void ProcessMessage(string message)
    {
        using var doc = JsonDocument.Parse(message);
        var root = doc.RootElement;

        switch (root.ValueKind)
        {
            case JsonValueKind.Array:
                ProcessDataMessage(root);
                break;
            case JsonValueKind.Object:
                ProcessInformationMessage(root);
                break;
        }
    }
    
    #region Helpers

    private void ProcessDataMessage(JsonElement root)
    {
        if (root[0].TryGetInt32(out var channelId) && _tradeChannels.ContainsValue(channelId))
        {
            if (root.GetArrayLength() > 2)
            {
                ProcessTradesUpdatedMessage(root);
            }
            else
            {
                ProcessTradesArrayMessage(root[1]);
            }
        }
        else if (_candleChannels.ContainsValue(channelId))
        {
            ProcessCandlesMessage(root[1]);
        }
    }

    private void ProcessTradesUpdatedMessage(JsonElement root)
    {
        var messageType = root[1].GetString() ?? string.Empty;
        
        if (_messageTypes.Contains(messageType))
        {
            var trade = JsonConverter.ConvertTradeToModel(_currentPair, root[2]);
            InvokeTradeEvents(trade);
        }
    }

    private void ProcessTradesArrayMessage(JsonElement tradeData)
    {
        if (tradeData.ValueKind == JsonValueKind.Array)
        {
            foreach (var trade in tradeData
                         .EnumerateArray()
                         .Select(tradeJson => JsonConverter.ConvertTradeToModel(_currentPair, tradeJson)))
            {
                InvokeTradeEvents(trade);
            }
        }
    }

    private void InvokeTradeEvents(Trade trade)
    {
        switch (trade.Side)
        {
            case "buy":
                NewBuyTrade?.Invoke(trade);
                break;
            case "sell":
                NewSellTrade?.Invoke(trade);
                break;
        }
    }

    private void ProcessCandlesMessage(JsonElement candleData)
    {
        if (candleData.ValueKind == JsonValueKind.Array)
        {
            if (candleData.GetArrayLength() > 6)
            {
                foreach (var candle in candleData
                             .EnumerateArray()
                             .Select(candleJson => JsonConverter.ConvertCandleToModel(_currentPair, candleJson)))
                {
                    CandleSeriesProcessing?.Invoke(candle);
                }
            }
            else
            {
                var candle = JsonConverter.ConvertCandleToModel(_currentPair, candleData);
                CandleSeriesProcessing?.Invoke(candle);
            }
        }
    }
    
    private void ProcessInformationMessage(JsonElement root)
    {
        if (root.TryGetProperty("event", out var eventProperty))
        {
            var eventType = eventProperty.GetString();
                
            if (eventType is "subscribe" or "unsubscribe")
            {
                _logger.LogDebug($"Received server event: {eventType}. Ignoring.");
            }
            else if (eventType is "subscribed")
            {
                if (root.TryGetProperty("chanId", out var chanIdProperty) && 
                    root.TryGetProperty("channel", out var channelProperty))
                {
                    var chanId = chanIdProperty.GetInt32();
                    var channel = channelProperty.GetString();
                        
                    switch (channel)
                    {
                        case "trades":
                            _tradeChannels[_currentPair] = chanId;
                            break;
                        case "candles":
                            _candleChannels[_currentPair] = chanId;
                            break;
                    }
                }
            }
        }
    }

    #endregion
}