using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class Dbinitializer
{
    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync("SearchDb", 
            MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();
        
        var count = await DB.CountAsync<Item>();

        if (count == 0)
        {
            Console.WriteLine("No data found - will insert data now");
            
            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetService<AuctionServiceHttpClient>();

            var items = await httpClient.GetItemsFromSearchDb();
            
            Console.WriteLine(items.Count + " Items count from Auction service");

            if (items.Count > 0) await DB.SaveAsync(items);
        }
    }
}
