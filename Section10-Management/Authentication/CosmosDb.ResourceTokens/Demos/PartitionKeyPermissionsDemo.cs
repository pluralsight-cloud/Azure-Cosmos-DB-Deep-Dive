using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CosmosDb.ResourceTokens.Demos
{
	public static class PartitionKeyPermissionsDemo
	{
		public async static Task Run()
		{
			Debugger.Break();

			// Create the database
			await Shared.Client.CreateDatabaseAsync("multi-tenant");
			var database = Shared.Client.GetDatabase("multi-tenant");

			// Create the container, partitioned on the tenantId
			await database.CreateContainerAsync("tenant-data", "/tenantId", 400);
			var container = database.GetContainer("tenant-data");

			// Create one user per tenant
			await database.CreateUserAsync("Acme");
			await database.CreateUserAsync("Wonka");

			// Create AcmePartition permission for Acme user to only access items in the Acme partition
			var acmeUser = database.GetUser("Acme");
			var acmePermissionProps = new PermissionProperties("AcmePartition", PermissionMode.All, container, new PartitionKey("Acme"));
			var acmePermission = (await acmeUser.CreatePermissionAsync(acmePermissionProps)).Resource;

			// Create WonkaPartition permission for Wonka user to only access items in the Wonka partition
			var wonkaUser = database.GetUser("Wonka");
			var wonkaPermissionProps = new PermissionProperties("WonkaPartition", PermissionMode.All, container, new PartitionKey("Wonka"));
			var wonkaPermission = (await wonkaUser.CreatePermissionAsync(wonkaPermissionProps)).Resource;

			// Define two documents; one for each tenant
			dynamic acmeDoc = new { id = "AcmeItem1", tenantId = "Acme", description = "Acme Item 1" };
			dynamic wonkaDoc = new { id = "WonkaItem1", tenantId = "Wonka", description = "Wonka Item 1" };

			// Try to create both documents using the resource token for Acme
			Console.WriteLine();
			Console.WriteLine(">>> Attempting to create documents using the Acme resource token <<<");
			await CreateDocument(acmePermission.Token, acmeDoc);
			await CreateDocument(acmePermission.Token, wonkaDoc);

			// Try to create both documents using the resource token for Wonka
			Console.WriteLine();
			Console.WriteLine(">>> Attempting to create documents using the Wonka resource token <<<");
			await CreateDocument(wonkaPermission.Token, acmeDoc);
			await CreateDocument(wonkaPermission.Token, wonkaDoc);

			// Delete the database
			await database.DeleteAsync();
		}

		private async static Task CreateDocument(string resourceToken, dynamic doc)
 		{
			// Get the endpoint to connect to Cosmos DB with a resource token rather than master key
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			var endpoint = config["CosmosEndpoint"];

			using (var client = new CosmosClient(endpoint, resourceToken))
			{
				var container = client.GetContainer("multi-tenant", "tenant-data");

				Console.WriteLine();
				Console.WriteLine($"Creating document with partition key {doc.tenantId}");
				try
				{
					await container.CreateItemAsync(doc, new PartitionKey(doc.tenantId));
					Console.WriteLine($"Successfully created document with partition key {doc.tenantId}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Unable to create document with partition key {doc.tenantId}: {ex.Message}");
				}
			}
		}

	}
}
