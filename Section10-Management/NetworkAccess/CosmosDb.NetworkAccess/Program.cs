using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace CosmosDb.NetworkAccess
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

            try
            {
                // Create a database
                var database = (await client.CreateDatabaseAsync("my-database")).Database;

                // Create a container
                var container = (await database.CreateContainerAsync("my-container", "/id", 400)).Container;

                // Write an item to the container
                var id = Guid.NewGuid().ToString();
                await container.CreateItemAsync(new { id, name = "Acme" });

                // Read it back
                var doc = await container.ReadItemAsync<dynamic>(id, new PartitionKey(id));

                // Delete the database and contanier
                await database.DeleteAsync();

                Console.WriteLine("All database operations completed successfully");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)     // 403 status
            {
                Console.WriteLine($"Access is blocked by Cosmos DB firewall or private endpoint: {ex.Message}");
            }
            catch (Exception ex)    // any other error
            {
                Console.WriteLine($"Unexpected error occurred: {ex.Message}");
            }
        }

    }
}
