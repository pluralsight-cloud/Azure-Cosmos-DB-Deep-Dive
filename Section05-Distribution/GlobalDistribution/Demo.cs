using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalDistribution
{
    public class Demo  // class Program						// <<-- on WUS and SEA VMs
    {
        public static async Task Menu()   // static async Task main		// <<-- on WUS and SEA VMs
        {
            while (true)
            {
                Console.WriteLine("Cosmos DB geo-replication demo");
                Console.WriteLine();
                Console.Write("Query / Read / Write / Conflict / Exit: ");
                var input = Console.ReadLine().Trim().ToUpper().Substring(0);

                if (input == "Q") await QueryDemo();
                if (input == "R") await ReadDemo();
                if (input == "W") await WriteDemo();

                if (input == "C") await ConflictDemo();

                if (input == "E") break;
            }
        }

        private static int GetCount()
        {
            while (true)
            {
                Console.Write("Iterations [100]: ");
                var countString = Console.ReadLine();
                if (countString.Trim().Length == 0)
                {
                    return 100;
                }
                if (int.TryParse(countString, out int count))
                {
                    return count;
                }
            }
        }

        private static CosmosClient GetClient()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var endpoint = config["CosmosEndpoint"];
            var masterKey = config["CosmosMasterKey"];
            var client = new CosmosClient(endpoint, masterKey);

            return client;
        }

        private static async Task QueryDemo()
        {
            var count = GetCount();

            using (var client = GetClient())
            {
                var totalStartedAt = DateTime.Now;
                var container = client.GetContainer("Families", "Families");
                for (var i = 0; i < count; i++)
                {
                    var iterator = container.GetItemQueryIterator<dynamic>(
                      queryText: "SELECT * FROM c WHERE c.address.zipCode = '60603'",
                      requestOptions: new QueryRequestOptions
                      { PartitionKey = new PartitionKey("60603") }
                    );
                    var startedAt = DateTime.Now;
                    var result = await iterator.ReadNextAsync();
                    var elapsed = DateTime.Now.Subtract(startedAt).TotalMilliseconds;
                    var cost = result.RequestCharge;

                    Console.WriteLine($"Query {i + 1}. {result.First().familyName}. {elapsed} ms; {cost} RUs");
                }
                var totalElapsed = DateTime.Now.Subtract(totalStartedAt).TotalMilliseconds;
                Console.WriteLine($"Total elapsed: {totalElapsed} ms");
                Console.WriteLine();
            }
        }

        private static async Task ReadDemo()
        {
            var count = GetCount();

            using (var client = GetClient())
            {
                var totalStartedAt = DateTime.Now;
                var container = client.GetContainer("Families", "Families");
                for (var i = 0; i < count; i++)
                {
                    var startedAt = DateTime.Now;

                    var result = await container.ReadItemAsync<dynamic>(
                        "Sample", new PartitionKey("60603"));

                    var elapsed = DateTime.Now.Subtract(startedAt).TotalMilliseconds;
                    var cost = result.RequestCharge;

                    Console.WriteLine($"Read {i + 1}. {result.Resource["familyName"]}. {elapsed} ms; {cost} RUs");
                }
                var totalElapsed = DateTime.Now.Subtract(totalStartedAt).TotalMilliseconds;
                Console.WriteLine($"Total elapsed: {totalElapsed} ms");
                Console.WriteLine();
            }
        }

        private static async Task WriteDemo()
        {
            var count = GetCount();

            using (var client = GetClient())
            {
                var container = client.GetContainer("Families", "Families");

                var readResult = await container.ReadItemAsync<dynamic>(
                    "Sample", new PartitionKey("60603"));

                dynamic doc = readResult.Resource;

                var totalStartedAt = DateTime.Now;
                for (var i = 0; i < count; i++)
                {
                    doc.familyName = $"Jones {Guid.NewGuid()}";
                    var startedAt = DateTime.Now;
                    var result = await container.ReplaceItemAsync(doc, "Sample");
                    var elapsed = DateTime.Now.Subtract(startedAt).TotalMilliseconds;
                    var cost = result.RequestCharge;

                    Console.WriteLine($"Write {i + 1}. {doc.familyName}. {elapsed} ms; {cost} RUs");
                }
                var totalElapsed = DateTime.Now.Subtract(totalStartedAt).TotalMilliseconds;
                Console.WriteLine($"Total elapsed: {totalElapsed} ms");
                Console.WriteLine();
            }
        }

        private static async Task ConflictDemo()
        {
            var count = GetCount();

            using (var client = GetClient())
            {
                var container = client.GetContainer("Families", "Families");
                for (var i = 0; i < count; i++)
                {
                    var readResult = await container.ReadItemAsync<dynamic>(
                        "Sample", new PartitionKey("60603"));

                    dynamic doc = readResult.Resource;

                    doc.familyName = $"Jones {Environment.MachineName} {i}";

                    if (i == 0)
                    {
                        Console.WriteLine($"Waiting {60 - DateTime.Now.Second} seconds for next minute interval for simultaneous update");
                        while (DateTime.Now.Second != 0) { }
                    }

                    await container.ReplaceItemAsync(doc, "Sample");

                    Console.WriteLine($"Updated Jones document with machine name '{Environment.MachineName} {i}'");
                }
                Console.WriteLine();
            }
        }

    }
}
