using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CosmosDb.DotNetSdk.Demos
{
	public static class StreamingIteratorsDemo
	{
		public async static Task Run()
		{
			Debugger.Break();

			await QueryWithServerSidePagingStreamed();
			await QueryWithClientSidePagingStreamed();
		}

		private async static Task QueryWithServerSidePagingStreamed()
		{
			Console.Clear();
			Console.WriteLine(">>> Query Documents with Streaming <<<");
			Console.WriteLine();

			var container = Shared.Client.GetContainer("adventure-works", "stores");
			var sql = "SELECT * FROM c";

			// Get all pages of large resultset using iterator.HasMoreResults
			Console.WriteLine("Querying for all documents (server-side paging, w/streaming iterator)");
			var streamIterator = container.GetItemQueryStreamIterator(sql, requestOptions: new QueryRequestOptions { MaxItemCount = 100 });
			var itemCount = 0;
			var pageCount = 0;
			while (streamIterator.HasMoreResults)
			{
				pageCount++;
				var results = await streamIterator.ReadNextAsync();
				var stream = results.Content;
				using (var sr = new StreamReader(stream))
				{
					var json = await sr.ReadToEndAsync();
					var jobj = JsonConvert.DeserializeObject<JObject>(json);
					var jarr = (JArray)jobj["Documents"];
					foreach (var item in jarr)
					{
						var customer = JsonConvert.DeserializeObject<Customer>(item.ToString());
						Console.WriteLine($" ({pageCount}.{++itemCount}) Id: {customer.Id}; Name: {customer.Name};");
					}
				}
			}
			Console.WriteLine($"Retrieved {itemCount} documents (server-side paging, w/streaming iterator");
			Console.WriteLine();
		}

		private async static Task QueryWithClientSidePagingStreamed()
		{
			// Get all pages of large resultset using continuation token
			Console.WriteLine("Querying for all documents (client-side paging, w/streaming iterator)");

			var continuationToken = default(string);
			do
			{
				continuationToken = await QueryFetchNextPageStreamed(continuationToken);
			} while (continuationToken != null);

			Console.WriteLine($"Retrieved all documents (client-side paging, w/streaming iterator)");
			Console.WriteLine();
		}

		private async static Task<string> QueryFetchNextPageStreamed(string continuationToken)
		{
			var container = Shared.Client.GetContainer("adventure-works", "stores");
			var sql = "SELECT * FROM c";

			var streamIterator = container.GetItemQueryStreamIterator(sql, continuationToken, new QueryRequestOptions { MaxItemCount = 100 });
			var response = await streamIterator.ReadNextAsync();

			var itemCount = 0;

			if (continuationToken != null)
			{
				Console.WriteLine($"... resuming with continuation {continuationToken}");
			}

			var stream = response.Content;
			using (var sr = new StreamReader(stream))
			{
				var json = await sr.ReadToEndAsync();
				var jobj = JsonConvert.DeserializeObject<JObject>(json);
				var jarr = (JArray)jobj["Documents"];
				foreach (var item in jarr)
				{
					var customer = JsonConvert.DeserializeObject<Customer>(item.ToString());
					Console.WriteLine($" ({++itemCount}) Id: {customer.Id}; Name: {customer.Name};");
				}
			}

			continuationToken = response.Headers.ContinuationToken;

			if (continuationToken == null)
			{
				Console.WriteLine($"... no more continuation; resultset complete");
			}

			return continuationToken;
		}

	}
}
