using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ThroughputTest
{
	class Program
	{
		static void Main(string[] args)
		{
			//Run1().Wait();
			Run2().Wait();
		}

		public static async Task Run2()
		{
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			var endpoint = config["CosmosEndpoint"];
			var masterKey = config["CosmosMasterKey"];


			var client = new CosmosClient(endpoint, masterKey);
			var container = client.GetContainer("Families", "Families");

			dynamic document = new
			{
				id = Guid.NewGuid(),
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

			var result = await container.CreateItemAsync(document, new PartitionKey(document.address.zipCode));
			var consumedRUs = result.RequestCharge;

			Console.WriteLine($"Cost to create document: {consumedRUs} RUs");





			var result2 = await container.ReplaceItemAsync(document, document.id.ToString(), new PartitionKey(document.address.zipCode));
			consumedRUs = result2.RequestCharge;
			Console.WriteLine($"Cost to replace document: {consumedRUs} RUs");


			var result3 = await container.DeleteItemAsync<dynamic>(document.id.ToString(), new PartitionKey(document.address.zipCode));
			consumedRUs = result3.RequestCharge;
			Console.WriteLine($"Cost to delete document: {consumedRUs} RUs");
		}

		public static async Task Run1()
		{
			var random = new Random();
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			var endpoint = config["CosmosEndpoint"];
			var masterKey = config["CosmosMasterKey"];

			var client = new CosmosClient(endpoint, masterKey);

			var database = (await client.CreateDatabaseIfNotExistsAsync("throughput-test")).Database;
			var container = (await database.CreateContainerIfNotExistsAsync("test-container", "/pk", 400)).Container;
			var threadCount = 3;
			var documentCount = 10000;

			var tasks = new List<Task>();
			for (var threadIndex = 0; threadIndex < threadCount; threadIndex++)
			{
				var ti = threadIndex;
				tasks.Add(Task.Run(async () =>
				{
					for (var di = 0; di < documentCount; di++)
					{
						var docIndex = di;
						if (docIndex % 100 == 0)
						{
							Console.WriteLine($"Doc {docIndex}, thread {threadIndex}");
							var delay = random.Next(1, 1000);
							await Task.Delay(delay);
						}
						var docDef = new
						{
							pk = Guid.NewGuid(),
							id = Guid.NewGuid(),
							name = $"Document {docIndex + 1} on thread {threadIndex}",
							content1 = new string('x', 500),
							content2 = new string('y', 500),
						};







						try
						{
							await container.CreateItemAsync(docDef, new PartitionKey(docDef.pk.ToString()));
						}
						catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests) // 429
						{
							Console.WriteLine($"Can't create document; request was throttled");
						}
















					}
				}));
			}

			Task.WaitAll(tasks.ToArray());
		}

	}

}
