﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDb.DotNetSdk.Demos
{
	public static class DocumentsDemo
	{
		public async static Task Run()
		{
			Debugger.Break();

			await CreateDocuments();

			await QueryDocuments();

			await QueryWithServerSidePaging();
			await QueryWithClientSidePaging();

			await QueryWithLinq();

			await ReplaceDocuments();

			await DeleteDocuments();
		}

		private async static Task CreateDocuments()
		{
			Console.Clear();
			Console.WriteLine(">>> Create Documents <<<");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "stores");

			dynamic document1Dynamic = new
			{
				id = Guid.NewGuid(),
				name = "New Customer 1",
				address = new
				{
					addressType = "Main Office",
					addressLine1 = "123 Main Street",
					location = new
					{
						city = "Brooklyn",
						stateProvinceName = "New York"
					},
					postalCode = "11229",
					countryRegionName = "United States"
				},
			};

			await container.CreateItemAsync(document1Dynamic, new PartitionKey("11229"));
			Console.WriteLine($"Created new document {document1Dynamic.id} from dynamic object");

			var document2Json = $@"
				{{
					""id"": ""{Guid.NewGuid()}"",
					""name"": ""New Customer 2"",
					""address"": {{
						""addressType"": ""Main Office"",
						""addressLine1"": ""123 Main Street"",
						""location"": {{
							""city"": ""Brooklyn"",
							""stateProvinceName"": ""New York""
						}},
						""postalCode"": ""11229"",
						""countryRegionName"": ""United States""
					}}
				}}";

			var document2Object = JsonConvert.DeserializeObject<JObject>(document2Json);
			await container.CreateItemAsync(document2Object, new PartitionKey("11229"));
			Console.WriteLine($"Created new document {document2Object["id"].Value<string>()} from JSON string");

			var document3Poco = new Customer
			{
				Id = Guid.NewGuid().ToString(),
				Name = "New Customer 3",
				Address = new Address
				{
					AddressType = "Main Office",
					AddressLine1 = "123 Main Street",
					Location = new Location
					{
						City = "Brooklyn",
						StateProvinceName = "New York"
					},
					PostalCode = "11229",
					CountryRegionName = "United States"
				},
			};

			await container.CreateItemAsync(document3Poco, new PartitionKey("11229"));
			Console.WriteLine($"Created new document {document3Poco.Id} from typed object");
		}

		private static async Task QueryDocuments()
		{
			Console.Clear();
			Console.WriteLine(">>> Query Documents (SQL) <<<");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "stores");

			Console.WriteLine("Querying for new customer documents (SQL)");
			var sql = "SELECT * FROM c WHERE STARTSWITH(c.name, 'New Customer') = true";

			// Query for dynamic objects
			var iterator1 = container.GetItemQueryIterator<dynamic>(sql);
			var documents1 = await iterator1.ReadNextAsync();
			var count = 0;
			foreach (var document in documents1)
			{
				Console.WriteLine($" ({++count}) Id: {document.id}; Name: {document.name};");

				// Dynamic object can be converted into a defined type...
				var customer = JsonConvert.DeserializeObject<Customer>(document.ToString());
				Console.WriteLine($"     City: {customer.Address.Location.City}");
			}
			Console.WriteLine($"Retrieved {count} new documents as dynamic");
			Console.WriteLine();

			// Or query for defined types; e.g., Customer
			var iterator2 = container.GetItemQueryIterator<Customer>(sql);
			var documents2 = await iterator2.ReadNextAsync();
			count = 0;
			foreach (var customer in documents2)
			{
				Console.WriteLine($" ({++count}) Id: {customer.Id}; Name: {customer.Name};");
				Console.WriteLine($"     City: {customer.Address.Location.City}");
			}
			Console.WriteLine($"Retrieved {count} new documents as Customer");
			Console.WriteLine();

			// You only get back the first "page" (up to MaxItemCount)
		}

		private async static Task QueryWithServerSidePaging()
		{
			Console.Clear();
			Console.WriteLine(">>> Query Documents (server-side paging) <<<");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "stores");
			var sql = "SELECT * FROM c";

			// Get first page of large resultset
			Console.WriteLine("Querying for all documents (first page)");
			var iterator = container.GetItemQueryIterator<Customer>(sql, requestOptions: new QueryRequestOptions { MaxItemCount = 100 });
			var documents = await iterator.ReadNextAsync();
			var itemCount = 0;
			foreach (var customer in documents)
			{
				Console.WriteLine($" ({++itemCount}) Id: {customer.Id}; Name: {customer.Name};");
			}
			Console.WriteLine($"Retrieved {itemCount} documents in first page");
			Console.WriteLine();

			// Get all pages of large resultset using iterator.HasMoreResults
			Console.WriteLine("Querying for all documents (full resultset, stateful)");
			iterator = container.GetItemQueryIterator<Customer>(sql, requestOptions: new QueryRequestOptions { MaxItemCount = 100 });
			itemCount = 0;
			var pageCount = 0;
			while (iterator.HasMoreResults)
			{
				pageCount++;
				documents = await iterator.ReadNextAsync();
				foreach (var customer in documents)
				{
					Console.WriteLine($" ({pageCount}.{++itemCount}) Id: {customer.Id}; Name: {customer.Name};");
				}
			}

			Console.WriteLine($"Retrieved {itemCount} documents (server-side paging)");
			Console.WriteLine();
		}

		private async static Task QueryWithClientSidePaging()
		{
			// Get all pages of large resultset using continuation token
			Console.WriteLine("Querying for all documents (client-side paging)");

			var continuationToken = default(string);
			do
			{
				continuationToken = await QueryFetchNextPage(continuationToken);
			} while (continuationToken != null);

			Console.WriteLine($"Retrieved all documents (client-side paging)");
			Console.WriteLine();
		}

		private async static Task<string> QueryFetchNextPage(string continuationToken)
		{
			var container = Shared.Client.GetContainer("adventure-works", "stores");
			var sql = "SELECT * FROM c";

			var iterator = container.GetItemQueryIterator<Customer>(sql, continuationToken, new QueryRequestOptions { MaxItemCount = 100 });
			var page = await iterator.ReadNextAsync();
			var itemCount = 0;

			if (continuationToken != null)
			{
				Console.WriteLine($"... resuming with continuation {continuationToken}");
			}

			foreach (var customer in page)
			{
				Console.WriteLine($" ({++itemCount}) Id: {customer.Id}; Name: {customer.Name};");
			}

			continuationToken = page.ContinuationToken;

			if (continuationToken == null)
			{
				Console.WriteLine($"... no more continuation; resultset complete");
			}

			return continuationToken;
		}

		private async static Task QueryWithLinq()
		{
			Console.Clear();
			Console.WriteLine(">>> Query Documents (LINQ) <<<");
			Console.WriteLine();

			Console.WriteLine("Querying for UK customers (LINQ)");
			var container = Shared.Client.GetContainer("adventure-works", "stores");

			// Call .ToFeedIterator on .GetItemLinqQueryable using LINQ query expression methods:
			using var iterator = container
				.GetItemLinqQueryable<Customer>()
				.Where(c => c.Address.CountryRegionName == "United Kingdom")
				.Select(c => new
				{
					c.Id,
					c.Name,
					c.Address.Location.City
				})
				.ToFeedIterator();

			//// Or call .ToFeedIterator on .GetItemLinqQueryable using LINQ query syntax:
			//using var iterator = (
			//	from c in container.GetItemLinqQueryable<Customer>()
			//	where c.Address.CountryRegionName == "United Kingdom"
			//	select new
			//	{
			//		c.Id,
			//		c.Name,
			//		c.Address.Location.City
			//	})
			//	.ToFeedIterator();

			//// Never call .ToList (synchronous):
			//var documents = (
			//	from d in container.GetItemLinqQueryable<Customer>(allowSynchronousQueryExecution: true)
			//	where d.Address.CountryRegionName == "United Kingdom"
			//	select new
			//	{
			//		d.Id,
			//		d.Name,
			//		d.Address.Location.City
			//	})
			//	.ToList();

			while (iterator.HasMoreResults)
			{
				foreach (var item in await iterator.ReadNextAsync())
				{
					{
						Console.WriteLine($" Id: {item.Id}; Name: {item.Name}; City: {item.City}");
					}
				}
			}
		}

		private async static Task ReplaceDocuments()
		{
			Console.Clear();
			Console.WriteLine(">>> Replace Documents <<<");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "stores");

			Console.WriteLine("Querying for documents with 'isNew' flag");
			var sql = "SELECT VALUE COUNT(c) FROM c WHERE c.isNew = true";
			var count = (await container.GetItemQueryIterator<int>(sql).ReadNextAsync()).First();
			Console.WriteLine($"Documents with 'isNew' flag: {count}");
			Console.WriteLine();

			Console.WriteLine("Querying for documents to be updated");
			sql = "SELECT * FROM c WHERE STARTSWITH(c.name, 'New Customer') = true";
			var documents = (await (container.GetItemQueryIterator<dynamic>(sql)).ReadNextAsync()).ToList();
			Console.WriteLine($"Found {documents.Count} documents to be updated");
			foreach (var document in documents)
			{
				document.isNew = true;
				var result = await container.ReplaceItemAsync<dynamic>(document, (string)document.id);
				var updatedDocument = result.Resource;
				Console.WriteLine($"Updated document 'isNew' flag: {updatedDocument.isNew}");
			}
			Console.WriteLine();

			Console.WriteLine("Querying for documents with 'isNew' flag");
			sql = "SELECT VALUE COUNT(c) FROM c WHERE c.isNew = true";
			count = (await container.GetItemQueryIterator<int>(sql).ReadNextAsync()).First();
			Console.WriteLine($"Documents with 'isNew' flag: {count}");
			Console.WriteLine();

			// Patch replace (partial document update)
			foreach (var document in documents)
			{
				var result = await container.PatchItemAsync<dynamic>(
					(string)document.id, new PartitionKey("11229"), new[]
					{ PatchOperation.Add("/isNew2", true) });

				var updatedDocument = result.Resource;
				Console.WriteLine($"Patched document 'isNew2' flag: {updatedDocument.isNew2}");
			}
		}

		private async static Task DeleteDocuments()
		{
			Console.Clear();
			Console.WriteLine(">>> Delete Documents <<<");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "stores");

			Console.WriteLine("Querying for documents to be deleted");
			var sql = "SELECT c.id, c.address.postalCode FROM c WHERE STARTSWITH(c.name, 'New Customer') = true";
			var iterator = container.GetItemQueryIterator<dynamic>(sql);
			var documents = (await iterator.ReadNextAsync()).ToList();
			Console.WriteLine($"Found {documents.Count} documents to be deleted");
			foreach (var document in documents)
			{
				string id = document.id;
				string pk = document.postalCode;
				await container.DeleteItemAsync<dynamic>(id, new PartitionKey(pk));
			}
			Console.WriteLine($"Deleted {documents.Count} new customer documents");
			Console.WriteLine();
		}

	}
}
