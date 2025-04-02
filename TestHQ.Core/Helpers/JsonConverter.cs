using System.Text.Json;
using TestHQ.Core.Models;

namespace TestHQ.Core.Helpers;

public static class JsonConverter
{
    public static Trade ConvertTradeToModel(string pair, JsonElement tradeJsonElement) =>
        new()
        {
            Id = tradeJsonElement[0].GetInt64().ToString(),
            Time = DateTimeOffset.FromUnixTimeMilliseconds(tradeJsonElement[1].GetInt64()),
            Amount = Math.Abs(tradeJsonElement[2].GetDecimal()),
            Price = tradeJsonElement[3].GetDecimal(),
            Side = tradeJsonElement[2].GetDecimal() > 0 ? "buy" : "sell",
            Pair = pair
        };

    public static Candle ConvertCandleToModel(string pair, JsonElement candleJsonElement) =>
        new() 
        {
            Pair = pair,
            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(candleJsonElement[0].GetInt64()),
            OpenPrice = candleJsonElement[1].GetDecimal(),
            ClosePrice = candleJsonElement[2].GetDecimal(),
            HighPrice = candleJsonElement[3].GetDecimal(),
            LowPrice = candleJsonElement[4].GetDecimal(),
            TotalVolume = candleJsonElement[5].GetDecimal()
        };

    public static IEnumerable<Trade> ConvertTradesToCollection(string pair, string json)
    {
        using var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;
        var tradeList = new List<Trade>();

        if (root.ValueKind == JsonValueKind.Array)
        {
            tradeList.AddRange(root.EnumerateArray().Select(tradeJson => ConvertTradeToModel(pair, tradeJson)));
        }

        return tradeList;
    }

    public static IEnumerable<Candle> ConvertCandlesToCollection(string pair, string json)
    {
        using var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;
        var candleList = new List<Candle>();

        if (root.ValueKind == JsonValueKind.Array)
        {
            candleList.AddRange(root.EnumerateArray().Select(tradeJson => ConvertCandleToModel(pair, tradeJson)));
        }

        return candleList;
    }

    public static Dictionary<string, decimal> ConvertTickersToPricesCollection(string json)
    {
        using var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;
        var prices = new Dictionary<string, decimal>();

        foreach (var arrayElement in root.EnumerateArray())
        {
            if (arrayElement.ValueKind == JsonValueKind.Array && arrayElement.GetArrayLength() >= 2)
            {
                var pair = arrayElement[0].GetString()!;
                var price = arrayElement[1].GetDecimal();

                prices.Add(pair, price);
            }
        }

        return prices;
    }
}