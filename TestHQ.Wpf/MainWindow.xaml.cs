using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.Options;
using TestHQ.Core.Clients;
using TestHQ.Core.Configurations;
using TestHQ.Wpf.Models;

namespace TestHQ.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadData();
    }
    
    private async void LoadData()
    {
        var bitfinexOptions = Options.Create(new BitfinexConfiguration
        {
            BaseUrl = "https://api-pub.bitfinex.com/v2",
            WsUrl = "wss://api.bitfinex.com/ws/2"
        });
        var client = new BitfinexRestClient(new HttpClient(), bitfinexOptions);
        var prices = await client.GetTickersAsync(["tBTCUSD", "tXRPUSD", "tXMRUSD", "tDSHUSD"]);
        var portfolio = CalculatePortfolio(prices);
        var portfolioList = CalculateTotalSum(portfolio, prices);

        PortfolioDataGrid.ItemsSource = portfolio;
        PortfolioTotalDataGrid.ItemsSource = portfolioList;
        
    }
    
    private static List<CryptoBalance> CalculatePortfolio(Dictionary<string, decimal> prices)
    {
        var portfolio = new List<CryptoBalance>
        {
            new() { Currency = "BTC", Amount = 1, ValueInUSD = prices["tBTCUSD"] },
            new() { Currency = "XRP", Amount = 15000, ValueInUSD = 15000 * prices["tXRPUSD"] },
            new() { Currency = "XMR", Amount = 50, ValueInUSD = 50 * prices["tXMRUSD"] },
            new() { Currency = "DASH", Amount = 30, ValueInUSD = 30 * prices["tDSHUSD"] }
        };

        return portfolio;
    }

    private static List<PortfolioItem> CalculateTotalSum(
        List<CryptoBalance> portfolio,
        Dictionary<string, decimal> prices)
    {
        var totalValueInUSD = portfolio.Sum(asset => asset.ValueInUSD);
        var totalValueInBTC = totalValueInUSD / prices["tBTCUSD"];
        var totalValueInXRP = totalValueInUSD / prices["tXRPUSD"];
        var totalValueInXMR = totalValueInUSD / prices["tXMRUSD"];
        var totalValueInDSH = totalValueInUSD / prices["tDSHUSD"];
        
        var portfolioList = new List<PortfolioItem>
        {
            new() {Token = "USD", Price = Math.Round(totalValueInUSD, 2)},
            new() {Token = "BTC", Price = Math.Round(totalValueInBTC, 2)},
            new() {Token = "XRP", Price = Math.Round(totalValueInXRP, 2)},
            new() {Token = "XMR", Price = Math.Round(totalValueInXMR, 2)},
            new() {Token = "DSH", Price = Math.Round(totalValueInDSH, 2)}
        };

        return portfolioList;
    }
}