using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace ComosDb.ChangeFeed
{
	public static class SyncProductCategory
	{
		private static CosmosClient Client { get; set; }

		static SyncProductCategory()
		{
			var connStr = Environment.GetEnvironmentVariable("CosmosDbConnectionString");
			Client = new CosmosClient(connStr);
		}

		[FunctionName("SyncProductCategoryName")]
		public static async Task SyncProductCategoryName(
			[CosmosDBTrigger(
				databaseName: "my-store",
				containerName: "productCategory",
				Connection = "CosmosDbConnectionString",
				LeaseContainerName = "lease",
				CreateLeaseContainerIfNotExists = true
			)]
			string documentsJson,
			ILogger log)
		{
			var documents = JsonConvert.DeserializeObject<JArray>(documentsJson);
			log.LogInformation($"Change detected in {documents.Count} product category document(s)");
			foreach (var document in documents)
			{
				var item = JsonConvert.DeserializeObject<dynamic>(document.ToString());
				string categoryId = item.id;
				string categoryName = item.name;

				var sql = $"SELECT * FROM c WHERE c.categoryId = '{categoryId}'";
				var productContainer = Client.GetContainer("my-store", "product");
				var options = new QueryRequestOptions { PartitionKey = new Microsoft.Azure.Cosmos.PartitionKey(categoryId) };
				var iterator = productContainer.GetItemQueryIterator<dynamic>(sql, requestOptions: options);
				var ctr = 0;
				while (iterator.HasMoreResults)
				{
					var page = await iterator.ReadNextAsync();
					foreach (var productDocument in page)
					{
						log.LogInformation($" Updating product ID {productDocument.id} with new category name '{categoryName}'");
						productDocument.categoryName = categoryName;
						await productContainer.ReplaceItemAsync(productDocument, productDocument.id.ToString());
						ctr++;
					}
				}

				log.LogInformation($"Propagated new category name '{categoryName}' (id '{categoryId}') to {ctr} product document(s)");
			}
		}

	}
}
