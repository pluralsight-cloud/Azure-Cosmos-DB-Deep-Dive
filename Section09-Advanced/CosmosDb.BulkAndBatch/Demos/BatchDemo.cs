using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CosmosDb.BulkAndBatch.Demos
{
	public static class BatchDemo
	{
		public async static Task Run()
		{
			Debugger.Break();

			Console.WriteLine();
			Console.WriteLine(">>> Transactional Batch <<<");
			Console.WriteLine();

			await CreateContainer();

			await CreateCustomerWithOrders();
			await CreateAnotherOrder();
			await CreateBadOrder();

			await DeleteContainer();
		}

		private async static Task CreateContainer()
		{
			Console.WriteLine("Creating batch-demo container");
			Console.WriteLine();

			var containerDef = new ContainerProperties
			{
				Id = "batch-demo",
				PartitionKeyPath = "/customerId",
			};

			var database = Shared.Client.GetDatabase("adventure-works");
			await database.CreateContainerAsync(containerDef, 400);
		}

		private async static Task CreateCustomerWithOrders()
		{
			Console.WriteLine("Creating customer with two orders");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "batch-demo");

			var customerId = "cust1";
			var newCustomerDoc = new Customer { Id = "cust1", CustomerId = customerId, Name = "John Doe", OrderCount = 2 };
			var newOrderDoc1 = new Order { Id = "order1", CustomerId = customerId, Item = "Surface Pro", Quantity = 1 };
			var newOrderDoc2 = new Order { Id = "order2", CustomerId = customerId, Item = "Surface Book", Quantity = 4 };

			var batch = container.CreateTransactionalBatch(new PartitionKey(customerId))
			  .CreateItem(newCustomerDoc)
			  .CreateItem(newOrderDoc1)
			  .CreateItem(newOrderDoc2);

			await ExecuteBatch(batch);
		}

		private async static Task CreateAnotherOrder()
		{
			Console.WriteLine("Adding another order");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "batch-demo");

			var customerId = "cust1";
			var result = await container.ReadItemAsync<Customer>("cust1", new PartitionKey(customerId));

			var existingCustomerDoc = result.Resource;

			existingCustomerDoc.OrderCount++;
			var newOrderDoc = new Order { Id = "order3", CustomerId = customerId, Item = "Surface Mouse", Quantity = 3 };

			var batch = container.CreateTransactionalBatch(new PartitionKey(customerId))
				.ReplaceItem(existingCustomerDoc.Id, existingCustomerDoc)
				.CreateItem(newOrderDoc);

			await ExecuteBatch(batch);
		}

		private async static Task CreateBadOrder()
		{
			Console.WriteLine("Adding another order (fails as duplicate)");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "batch-demo");

			var customerId = "cust1";
			var result = await container.ReadItemAsync<Customer>("cust1", new PartitionKey(customerId));

			var existingCustomerDoc = result.Resource;

			existingCustomerDoc.OrderCount++;
			var newOrderDocDupe = new Order { Id = "order2", CustomerId = customerId, Item = "Surface Keyboard", Quantity = 2 };

			var batch = container.CreateTransactionalBatch(new PartitionKey(customerId))
				.ReplaceItem(existingCustomerDoc.Id, existingCustomerDoc)
				.CreateItem(newOrderDocDupe);

			await ExecuteBatch(batch);
		}

		private static async Task ExecuteBatch(TransactionalBatch batch)
		{
			var batchResponse = await batch.ExecuteAsync();

			using (batchResponse)
			{
				if (batchResponse.IsSuccessStatusCode)
				{
					Console.WriteLine("Transcational batch succeeded");
					for (var i = 0; i < batchResponse.Count; i++)
					{
						var result = batchResponse.GetOperationResultAtIndex<dynamic>(i);
						Console.WriteLine($"Document {i + 1}:");
						Console.WriteLine(result.Resource);
					}
				}
				else
				{
					Console.WriteLine("Transcational batch failed");
					for (var i = 0; i < batchResponse.Count; i++)
					{
						var result = batchResponse.GetOperationResultAtIndex<dynamic>(i);
						Console.WriteLine($"Document {i + 1}: {result.StatusCode}");
					}
				}
			}
		}

		private async static Task DeleteContainer()
		{
			Console.WriteLine("Deleting bulkdemo container");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "batch-demo");
			await container.DeleteContainerAsync();
		}

	}

	public class Customer
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("customerId")]
		public string CustomerId { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("orderCount")]
		public int OrderCount { get; set; }
	}

	public class Order
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("customerId")]
		public string CustomerId { get; set; }
		[JsonProperty("item")]
		public string Item { get; set; }
		[JsonProperty("quantity")]
		public int Quantity { get; set; }
	}

}
