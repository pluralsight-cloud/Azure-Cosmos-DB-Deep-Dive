using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

await QueryForDocuments();

static async Task QueryForDocuments()
{
	var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
	var endpoint = config["CosmosEndpoint"];
	var masterKey = config["CosmosMasterKey"];

	using var client = new CosmosClient(endpoint, masterKey);
	var container = client.GetContainer("Families", "Families");
	var sql = "SELECT * FROM c WHERE ARRAY_LENGTH(c.children) > 1";
	var iterator = container.GetItemQueryIterator<dynamic>(sql);
	var page = await iterator.ReadNextAsync();

	foreach (var doc in page)
	{
		Console.WriteLine($"Family {doc.id} has {doc.children.Count} children");
	}
}
