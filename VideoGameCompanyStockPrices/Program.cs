using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace VideoGameCompanyStockPrices
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new HttpClient();
            var apiKey = "0NGRHE8PEQBF33VY";  // Your API key

            // Stock symbols
            var nintendoSymbol = "7974.T";  // Nintendo
            var microsoftSymbol = "MSFT";   // Microsoft
            var sonySymbol = "6758.T";      // Sony

            Console.WriteLine("Do you want to (1) Fetch and store new data, or (2) Retrieve data from the database?");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                // Get stock data and store in the database
                await GetAndSaveStockData(client, apiKey, nintendoSymbol);
                await GetAndSaveStockData(client, apiKey, microsoftSymbol);
                await GetAndSaveStockData(client, apiKey, sonySymbol);
                Console.WriteLine("Data has been fetched and saved.");
            }
            else if (choice == "2")
            {
                // Ask the user which company's data they want to retrieve
                Console.WriteLine("Enter the company symbol (Nintendo: 7974.T, Microsoft: MSFT, Sony: 6758.T):");
                var symbol = Console.ReadLine().ToUpper();

                // Retrieve stock data from the database based on user input
                await RetrieveStockData(symbol);
            }
            else
            {
                Console.WriteLine("Invalid choice. Please restart the program.");
            }
        }

        static async Task GetAndSaveStockData(HttpClient client, string apiKey, string symbol)
        {
            // Get stock data from Alpha Vantage
            var response = await client.GetStringAsync($"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}");
            var stockData = JObject.Parse(response);

            // Parse the stock prices
            var firstDate = stockData["Time Series (Daily)"].First;
            var date = DateTime.Parse(firstDate.Path.Split('.').Last());
            var dailyData = firstDate.First;

            var openPrice = (decimal)dailyData["1. open"];
            var highPrice = (decimal)dailyData["2. high"];
            var lowPrice = (decimal)dailyData["3. low"];
            var closePrice = (decimal)dailyData["4. close"];
            var volume = (decimal)dailyData["5. volume"];

            // Save to the database
            using (var context = new ProductContext())
            {
                var stockEntity = new ProductEntity
                {
                    Symbol = symbol,
                    Date = date,
                    OpenPrice = openPrice,
                    HighPrice = highPrice,
                    LowPrice = lowPrice,
                    ClosePrice = closePrice,
                    Volume = volume
                };

                context.Products.Add(stockEntity);
                await context.SaveChangesAsync();
            }
        }

        static async Task RetrieveStockData(string symbol)
        {
            using (var context = new ProductContext())
            {
                // Query the database for stock data by symbol
                var stockData = await context.Products
                    .Where(p => p.Symbol == symbol)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefaultAsync();

                if (stockData != null)
                {
                    // Display the retrieved stock data
                    Console.WriteLine($"Symbol: {stockData.Symbol}");
                    Console.WriteLine($"Date: {stockData.Date.ToShortDateString()}");
                    Console.WriteLine($"Open Price: {stockData.OpenPrice}");
                    Console.WriteLine($"High Price: {stockData.HighPrice}");
                    Console.WriteLine($"Low Price: {stockData.LowPrice}");
                    Console.WriteLine($"Close Price: {stockData.ClosePrice}");
                    Console.WriteLine($"Volume: {stockData.Volume}");
                }
                else
                {
                    Console.WriteLine("No data found for the specified symbol.");
                }
            }
        }
    }
}
