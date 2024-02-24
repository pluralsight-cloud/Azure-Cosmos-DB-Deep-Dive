using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CrossPartitionQueries
{
	class Program
	{
		static void Main(string[] args)
		{
			RunSetup().Wait();
			RunDemo().Wait();
			RunCleanup().Wait();
		}

		public static async Task RunSetup()
		{
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			var endpoint = config["CosmosEndpoint"];
			var masterKey = config["CosmosMasterKey"];

			using (var client = new CosmosClient(endpoint, masterKey))
			{
				await client.GetDatabase("families").DeleteAsync();

				var database = (await client.CreateDatabaseAsync("families")).Database;
				var container = (await database.CreateContainerAsync("families", "/address/zipCode", 400)).Container;

				await AddChicago60601Document(client);  // City = Chicago, ZipCode = 60601
				await AddChicago60603Document(client);  // City = Chicago, ZipCode = 60603
			}
		}

		private static async Task AddChicago60601Document(CosmosClient client)
		{
			dynamic documentDef = new
			{
				id = Guid.NewGuid().ToString(),
				familyName = "Smith",
				address = new
				{
					addressLine = "123 Main Street",
					city = "Chicago",
					state = "IL",
					zipCode = "60601"
				},
				parents = new string[]
			  {
				  "Peter",
				  "Alice"
			  },
				kids = new string[]
			  {
				  "Adam",
				  "Jacqueline",
				  "Joshua"
			  }
			};

			var container = client.GetContainer("families", "families");
			var result = await container.CreateItemAsync(documentDef, new PartitionKey(documentDef.address.zipCode));
			var document = result.Resource;
			Console.WriteLine($"Created Smith document with ID: {document.id}");
		}

		private static async Task AddChicago60603Document(CosmosClient client)
		{
			dynamic documentDef = new
			{
				id = Guid.NewGuid().ToString(),
				familyName = "Jones",
				address = new
				{
					addressLine = "456 Harbor Boulevard",
					city = "Chicago",
					state = "IL",
					zipCode = "60603"
				},
				parents = new string[]
			  {
				"David",
				"Diana"
			  },
				kids = new string[]
			  {
				"Evan"
			  },
				pets = new string[]
			  {
				"Lint"
			  }
			};

			var container = client.GetContainer("families", "families");
			var result = await container.CreateItemAsync(documentDef, new PartitionKey(documentDef.address.zipCode));
			var document = result.Resource;
			Console.WriteLine($"Created Jones document with ID: {document.id}");
		}

		public static async Task RunDemo()
		{
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			var endpoint = config["CosmosEndpoint"];
			var masterKey = config["CosmosMasterKey"];

			using (var client = new CosmosClient(endpoint, masterKey))
			{
				var container = client.GetContainer("families", "families");

				/* Always supply the partition key if known */
				var iterator = container.GetItemQueryIterator<dynamic>(
				  queryText: "SELECT * FROM c WHERE c.address.zipCode = '60603'",
				  requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey("60603") }
				);
				// Returns the one 60603 document from the 60603 partition
				var firstPage = (await iterator.ReadNextAsync()).ToList();
				Console.WriteLine();
				Console.WriteLine(string.Join<dynamic>(Environment.NewLine, firstPage.Select(d => new { d.id, d.address.zipCode, d.address.city, d.familyName }).ToArray()));

				/* For a cross-partition query, omit the partition key and optionally set the client parallelism */
				iterator = container.GetItemQueryIterator<dynamic>(
				  queryText: "SELECT * FROM c WHERE c.address.city = 'Chicago'",
				  requestOptions: new QueryRequestOptions { MaxConcurrency = -1 }
				);
				// Returns all Chicago documents from all partitions
				firstPage = (await iterator.ReadNextAsync()).ToList();
				Console.WriteLine();
				Console.WriteLine(string.Join<dynamic>(Environment.NewLine, firstPage.Select(d => new { d.id, d.address.zipCode, d.address.city, d.familyName }).ToArray()));

				/* Supplying the partition key always limits the query results to that partition key value */
				iterator = container.GetItemQueryIterator<dynamic>(
				  queryText: "SELECT * FROM c WHERE c.address.city = 'Chicago'",
				  requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey("60601") }
				);
				// Returns all Chicago documents from the 60601 partition (just one document; excludes the 60603 document)
				firstPage = (await iterator.ReadNextAsync()).ToList();
				Console.WriteLine();
				Console.WriteLine(string.Join<dynamic>(Environment.NewLine, firstPage.Select(d => new { d.id, d.address.zipCode, d.address.city, d.familyName }).ToArray()));
			}
		}

		public static async Task RunCleanup()
		{
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			var endpoint = config["CosmosEndpoint"];
			var masterKey = config["CosmosMasterKey"];

			using (var client = new CosmosClient(endpoint, masterKey))
			{
				await client.GetDatabase("families").DeleteAsync();
			}
		}

	}
}
