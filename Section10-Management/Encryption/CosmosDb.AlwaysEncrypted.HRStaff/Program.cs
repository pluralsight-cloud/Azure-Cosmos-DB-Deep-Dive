using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Encryption;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CosmosDb.AlwaysEncrypted.HRStaff
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Debugger.Break();

			// Get access to the configuration file
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

			// Get the Cosmos DB account endpoint and master key
			var endpoint = config["CosmosEndpoint"];
			var masterKey = config["CosmosMasterKey"];

			// Get Entra ID tenant ID, plus the client ID and secret for the HR Staff application
			var tenantId = config["TenantId"];
			var clientId = config["ClientId"];
			var clientSecret = config["ClientSecret"];

			// Create an Azure Key Vault key store provider from the Entra ID tenant ID with the client ID and client secret
			var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
			var keyResolver = new KeyResolver(credential);

			// Create a Cosmos client with Always Encrypted enabled using the key store provider
			var client = new CosmosClient(endpoint, masterKey)
				.WithEncryption(keyResolver, KeyEncryptionKeyResolverName.AzureKeyVault);

			// Get the employees container
			var container = client.GetContainer("human-resources", "employees");

			// Try to retrieve documents with all properties
			Console.WriteLine("Retrieving documents with all properties");
			try
			{
				// Fails because the HR Staff application is not listed in the access policy for the SSN Azure Key Vault
				await container.GetItemQueryIterator<dynamic>("SELECT * FROM C").ReadNextAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to retrieve documents with all properties");
				Console.WriteLine(ex.Message);
				Console.WriteLine("Press any key to continue");
				Console.ReadKey();
				Console.WriteLine();
			}

			// Succeeds because we are excluding the SSN property in the query projection
			Console.WriteLine("Retrieving documents without the SSN property");
			var results = await container.GetItemQueryIterator<dynamic>(
				"SELECT c.id, c.firstName, c.LastName, c.department, c.salary FROM c").ReadNextAsync();

			Console.WriteLine("Retrieved documents without the SSN property");
			foreach (var result in results)
			{
				Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
			}

			// Delete the database
			var database = client.GetDatabase("human-resources");
			await database.DeleteAsync();
		}

	}
}
