using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CosmosDb.ServerSide.Demos
{
    public static class StoredProceduresDemo
	{
		public async static Task Run()
		{
			Debugger.Break();

			await CreateStoredProcedures();

			await ViewStoredProcedures();

			await ExecuteStoredProcedures();

			await DeleteStoredProcedures();
		}

		// Create stored procedures

		private async static Task CreateStoredProcedures()
		{
			Console.WriteLine();
			Console.WriteLine(">>> Create Stored Procedures <<<");
			Console.WriteLine();

			await CreateStoredProcedure("spHelloWorld");
			await CreateStoredProcedure("spSetNorthAmerica");
		}

		private async static Task CreateStoredProcedure(string sprocId)
		{
			var sprocBody = File.ReadAllText($@"Server\{sprocId}.js");

			var sprocProps = new StoredProcedureProperties
			{
				Id = sprocId,
				Body = sprocBody
			};

			var container = Shared.Client.GetContainer("adventure-works", "stores");
			var result = await container.Scripts.CreateStoredProcedureAsync(sprocProps);
			Console.WriteLine($"Created stored procedure {sprocId} ({result.RequestCharge} RUs);");
		}

		// View stored procedures

		private static async Task ViewStoredProcedures()
		{
			Console.WriteLine();
			Console.WriteLine(">>> View Stored Procedures <<<");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "stores");

			var iterator = container.Scripts.GetStoredProcedureQueryIterator<StoredProcedureProperties>();
			var sprocs = await iterator.ReadNextAsync();

			var count = 0;
			foreach (var sproc in sprocs)
			{
				count++;
				Console.WriteLine($" Stored procedure Id: {sproc.Id}; Modified: {sproc.LastModified}");
			}

			Console.WriteLine();
			Console.WriteLine($"Total stored procedures: {count}");
		}

		// Execute stored procedures

		private async static Task ExecuteStoredProcedures()
		{
			Console.Clear();
			await Execute_spHelloWorld();

			Console.Clear();
			await Execute_spSetNorthAmerica1();
			await Execute_spSetNorthAmerica2();
			await Execute_spSetNorthAmerica3();
		}

		private async static Task Execute_spHelloWorld()
		{
			Console.WriteLine();
			Console.WriteLine("Execute spHelloWorld stored procedure");

			var scripts = Shared.Client.GetContainer("adventure-works", "stores").Scripts;
			var pk = new PartitionKey(string.Empty);
			var result = await scripts.ExecuteStoredProcedureAsync<string>("spHelloWorld", pk, null);
			var message = result.Resource;

			Console.WriteLine($"Result: {message}");
		}

		private async static Task Execute_spSetNorthAmerica1()
		{
			Console.WriteLine();
			Console.WriteLine("Execute spSetNorthAmerica (country = United States)");

			// Should succeed with isNorthAmerica = true
			var id = Guid.NewGuid().ToString();
			dynamic documentDefinition = new
			{
				id,
				name = "John Doe",
				address = new
				{
					countryRegionName = "United States",
					postalCode = "12345"
				}
			};

			var container = Shared.Client.GetContainer("adventure-works", "stores");
			var pk = new PartitionKey(documentDefinition.address.postalCode);
			var result = await container.Scripts.ExecuteStoredProcedureAsync<dynamic>("spSetNorthAmerica", pk, new[] { documentDefinition, true });
			var document = result.Resource;

			var country = document.address.countryRegionName;
			var isNA = document.address.isNorthAmerica;

			Console.WriteLine("Result:");
			Console.WriteLine($" Country = {country}");
			Console.WriteLine($" Is North America = {isNA}");

			await container.DeleteItemAsync<dynamic>(id, pk);
		}

		private async static Task Execute_spSetNorthAmerica2()
		{
			Console.WriteLine();
			Console.WriteLine("Execute spSetNorthAmerica (country = United Kingdom)");

			// Should succeed with isNorthAmerica = false
			var id = Guid.NewGuid().ToString();
			dynamic documentDefinition = new
			{
				id,
				name = "John Doe",
				address = new
				{
					countryRegionName = "United Kingdom",
					postalCode = "RG41 1QW"
				}
			};

			var container = Shared.Client.GetContainer("adventure-works", "stores");
			var pk = new PartitionKey(documentDefinition.address.postalCode);
			var result = await container.Scripts.ExecuteStoredProcedureAsync<dynamic>("spSetNorthAmerica", pk, new[] { documentDefinition, true });
			var document = result.Resource;

			// Deserialize new document as JObject (use dictionary-style indexers to access dynamic properties)
			var documentObject = JsonConvert.DeserializeObject(document.ToString());

			var country = documentObject["address"]["countryRegionName"];
			var isNA = documentObject["address"]["isNorthAmerica"];

			Console.WriteLine("Result:");
			Console.WriteLine($" Country = {country}");
			Console.WriteLine($" Is North America = {isNA}");

			await container.DeleteItemAsync<dynamic>(id, pk);
		}

		private async static Task Execute_spSetNorthAmerica3()
		{
			Console.WriteLine();
			Console.WriteLine("Execute spSetNorthAmerica (no country)");

			var id = Guid.NewGuid().ToString();
			dynamic documentDefinition = new
			{
				id,
				name = "James Smith",
				address = new
				{
					postalCode = "12345"
				}
			};

			var container = Shared.Client.GetContainer("adventure-works", "stores");
			var pk = new PartitionKey(documentDefinition.address.postalCode);

			try
			{
				// Should fail with no country and enforceSchema = true
				var result = await container.Scripts.ExecuteStoredProcedureAsync<dynamic>("spSetNorthAmerica", pk, new[] { documentDefinition, true });
			}
			catch (CosmosException ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}


		// Delete stored procedures

		private async static Task DeleteStoredProcedures()
		{
			Console.WriteLine();
			Console.WriteLine(">>> Delete Stored Procedures <<<");
			Console.WriteLine();

			await DeleteStoredProcedure("spHelloWorld");
			await DeleteStoredProcedure("spSetNorthAmerica");
		}

		private async static Task DeleteStoredProcedure(string sprocId)
		{
			var container = Shared.Client.GetContainer("adventure-works", "stores");
			await container.Scripts.DeleteStoredProcedureAsync(sprocId);

			Console.WriteLine($"Deleted stored procedure: {sprocId}");
		}

	}
}
