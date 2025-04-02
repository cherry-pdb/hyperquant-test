namespace TestHQ.Core.Models;

public class Candle
{
    /// <summary>
    /// Валютная пара
    /// </summary>
    public string Pair { get; set; }

    /// <summary>
    /// Цена открытия
    /// </summary>
    public decimal OpenPrice { get; set; }

    /// <summary>
    /// Максимальная цена
    /// </summary>
    public decimal HighPrice { get; set; }

    /// <summary>
    /// Минимальная цена
    /// </summary>
    public decimal LowPrice { get; set; }

    /// <summary>
    /// Цена закрытия
    /// </summary>
    public decimal ClosePrice { get; set; }

    /// <summary>
    /// Partial (Общий объем)
    /// </summary>
    public decimal TotalVolume { get; set; } // I removed field TotalPrice because it isn't in response

    /// <summary>
    /// Время
    /// </summary>
    public DateTimeOffset OpenTime { get; set; }
}