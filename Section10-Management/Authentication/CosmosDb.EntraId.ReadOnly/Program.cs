using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace CosmosDb.EntraId.ReadOnly
{
	public static class Program
    {
		private static async Task Main(string[] args)
		{
			Debugger.Break();

			Console.WriteLine();
			Console.WriteLine(">>> Read-Only Application <<<");
			Console.WriteLine();

			// Get access to the configuration file
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			// Get the Cosmos DB account endpoint
			var endpoint = config["CosmosEndpoint"];

			// Get Entra ID tenant ID, plus the client ID and secret for the read-only application
			var tenantId = config["TenantId"];
			var clientId = config["ClientId"];
			var clientSecret = config["ClientSecret"];

			// Create a Cosmos client from the AAD directory ID with the client ID and client secret
			var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
			using var client = new CosmosClient(endpoint, credential);

			// Get a reference to the container
			var container = client.GetContainer("iot-demo", "iot");

			// Fails because role definition does not allow creating documents
			try
			{
				var doc = new { id = "Event2", deviceId = "Device1", temperature = 110 };
				await container.CreateItemAsync(doc, new PartitionKey("Device1"));
			}
			catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Forbidden) // 403
			{
				Console.WriteLine("Permission denied for create document");
			}

			// Succeeds because role definition allows point reads
			await container.ReadItemAsync<dynamic>("Event1", new PartitionKey("Device1"));
			Console.WriteLine("Successfully executed point read");

			// Succeeds because role definition allows queries
			await container.GetItemQueryIterator<dynamic>("SELECT * FROM c").ReadNextAsync();
			Console.WriteLine("Successfully executed query");
		}

    }
}
