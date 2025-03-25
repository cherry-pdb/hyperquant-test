namespace TestHQ.Core.Helpers;

public static class PeriodConverter
{
    public static string GetPeriodString(int periodInSec)
    {
        if (periodInSec <= 0)
            throw new ArgumentOutOfRangeException(nameof(periodInSec), "Period in seconds must be a positive.");

        const int oneMinute = 60;
        const int fiveMinutes = 5 * oneMinute;
        const int fifteenMinutes = 15 * oneMinute;
        const int thirtyMinutes = 30 * oneMinute;
        const int oneHour = 60 * oneMinute;
        const int threeHours = 3 * oneHour;
        const int sixHours = 6 * oneHour;
        const int twelveHours = 12 * oneHour;
        const int oneDay = 24 * oneHour;
        const int oneWeek = 7 * oneDay;
        const int fourteenDays = 14 * oneDay;
        const int oneMonth = 30 * oneDay;

        return periodInSec switch
        {
            oneMinute => "1m",
            fiveMinutes => "5m",
            fifteenMinutes => "15m",
            thirtyMinutes => "30m",
            oneHour => "1h",
            threeHours => "3h",
            sixHours => "6h",
            twelveHours => "12h",
            oneDay => "1D",
            oneWeek => "1W",
            fourteenDays => "14D",
            oneMonth => "1M",
            _ => throw new ArgumentException("Invalid period in seconds for API. Check available values here: https://docs.bitfinex.com/reference/rest-public-candles")
        };
    }
}