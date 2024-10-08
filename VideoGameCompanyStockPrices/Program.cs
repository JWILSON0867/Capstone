﻿using System;
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

            /*// Ensure the database is deleted and created fresh
            using (var context = new ProductContext())
            {
                // Deletes the existing database if it exists
                context.Database.EnsureDeleted();

                // Ensures the database is created (if not exists)
                context.Database.EnsureCreated();
            }*/

            var client = new HttpClient();
            var apiKey = "0NGRHE8PEQBF33VY";  // Your API key

            // Stock symbols
            var nintendoSymbol = "NTDOY";  // Nintendo
            var microsoftSymbol = "MSFT";   // Microsoft
            var sonySymbol = "SONY";      // Sony

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
                Console.WriteLine("Enter the company symbol (Nintendo: NTDOY, Microsoft: MSFT, Sony: SONY):");
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

            // Log the response for debugging
            Console.WriteLine(response);

            var stockData = JObject.Parse(response);

            // Check if there is an error message
            if (stockData["Error Message"] != null)
            {
                Console.WriteLine($"Error retrieving data for {symbol}: {stockData["Error Message"]}");
                return; // Exit the method if there's an error
            }

            // Check if the expected property exists
            if (stockData["Time Series (Daily)"] == null)
            {
                Console.WriteLine("No Time Series data found. Please check the response.");
                return; // Exit the method if there's no data
            }

            // Proceed with parsing the stock prices
            var firstDate = stockData["Time Series (Daily)"].First;
            var date = DateTime.Parse(firstDate.Path.Split('.').Last());
            var dailyData = firstDate.First;

            var openPrice = (decimal)dailyData["1. open"];
            var highPrice = (decimal)dailyData["2. high"];
            var lowPrice = (decimal)dailyData["3. low"];
            var closePrice = (decimal)dailyData["4. close"];
            var volume = (decimal)dailyData["5. volume"];

            // Save to database
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
