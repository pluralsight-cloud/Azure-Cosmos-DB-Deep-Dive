using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CosmosDb.ChangeFeed.Setup
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Debugger.Break();

			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			var endpoint = config["CosmosEndpoint"];
			var masterKey = config["CosmosMasterKey"];

			var client = new CosmosClient(endpoint, masterKey);

			// Create my-store database
			var database = (await client.CreateDatabaseAsync("my-store")).Database;

			// Create productCategory container
			var pc = (await database.CreateContainerAsync("productCategory", "/type", 400)).Container;
			await pc.CreateItemAsync(new { id = "C-TS", type = "category", name = "T-shirts" }, new PartitionKey("category"));
			await pc.CreateItemAsync(new { id = "C-SH", type = "category", name = "Shorts" }, new PartitionKey("category"));

			// Create product container
			var p = (await database.CreateContainerAsync("product", "/categoryId", 400)).Container;
			await p.CreateItemAsync(new { id = "P1081", categoryId = "C-TS", categoryName = "T-shirts", name = "V-neck" }, new PartitionKey("C-TS"));
			await p.CreateItemAsync(new { id = "P1082", categoryId = "C-TS", categoryName = "T-shirts", name = "Crew neck" }, new PartitionKey("C-TS"));
			await p.CreateItemAsync(new { id = "P1083", categoryId = "C-SH", categoryName = "Shorts", name = "Bermuda shorts" }, new PartitionKey("C-SH"));
			await p.CreateItemAsync(new { id = "P1084", categoryId = "C-SH", categoryName = "Shorts", name = "Cycling shorts" }, new PartitionKey("C-SH"));
			await p.CreateItemAsync(new { id = "P1085", categoryId = "C-TS", categoryName = "T-shirts", name = "Polo collar" }, new PartitionKey("C-TS"));
			await p.CreateItemAsync(new { id = "P1086", categoryId = "C-SH", categoryName = "Shorts", name = "Boardshorts" }, new PartitionKey("C-SH"));

			Console.WriteLine("Successfully created my-store database for change feed demo");
		}

	}
}
