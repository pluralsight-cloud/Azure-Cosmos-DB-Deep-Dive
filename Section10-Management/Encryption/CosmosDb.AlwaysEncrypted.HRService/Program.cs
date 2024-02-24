using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Encryption;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDb.AlwaysEncrypted.HRService
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

			// Get Entra ID tenant ID, plus the client ID and secret for the HR Service application
			var tenantId = config["TenantId"];
			var clientId = config["ClientId"];
			var clientSecret = config["ClientSecret"];

			// Create an Azure Key Vault key store provider from the Entra ID tenant ID with the client ID and client secret
			var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
			var keyResolver = new KeyResolver(credential);

			// Create a Cosmos client with Always Encrypted enabled using the key store provider
			var client = new CosmosClient(endpoint, masterKey)
				.WithEncryption(keyResolver, KeyEncryptionKeyResolverName.AzureKeyVault);

			// Create the HR database
			await client.CreateDatabaseAsync("human-resources");
			var database = client.GetDatabase("human-resources");

			Console.WriteLine("Created human-resources database");

			// Create salary data encryption keys (DEK) from the salary customer-managed key (CMK) in AKV
			var salaryCmkId = config["AkvSalaryCmkId"];

			var salaryEncryptionKeyWrapMetadata = new EncryptionKeyWrapMetadata(
				type: KeyEncryptionKeyResolverName.AzureKeyVault,
				name: "akvMasterKey",
				value: salaryCmkId,
				algorithm: EncryptionAlgorithm.RsaOaep.ToString());

			await database.CreateClientEncryptionKeyAsync(
				clientEncryptionKeyId: "salaryDek",
				DataEncryptionAlgorithm.AeadAes256CbcHmacSha256,
				salaryEncryptionKeyWrapMetadata);

			Console.WriteLine("Created salary database encryption key");

			// Create SSN data encryption key (DEK) from the SSN customer-managed key (CMK) in AKV
			var ssnCmkId = config["AkvSsnCmkId"];

			var ssnEncryptionKeyWrapMetadata = new EncryptionKeyWrapMetadata(
				type: KeyEncryptionKeyResolverName.AzureKeyVault,
				name: "akvMasterKey",
				value: ssnCmkId,
				algorithm: EncryptionAlgorithm.RsaOaep.ToString());

			await database.CreateClientEncryptionKeyAsync(
				"ssnDek",
				DataEncryptionAlgorithm.AeadAes256CbcHmacSha256,
				ssnEncryptionKeyWrapMetadata);

			Console.WriteLine("Created SSN database encryption key");

			// Define a client-side encryption path for the salary property
			var path1 = new ClientEncryptionIncludedPath()
			{
				Path = "/salary",
				ClientEncryptionKeyId = "salaryDek",
				EncryptionAlgorithm = DataEncryptionAlgorithm.AeadAes256CbcHmacSha256.ToString(),
				EncryptionType = EncryptionType.Randomized,  // Most secure, but not queryable
			};

			// Define a client-side encryption path for the SSN property
			var path2 = new ClientEncryptionIncludedPath()
			{
				Path = "/ssn",
				ClientEncryptionKeyId = "ssnDek",
				EncryptionAlgorithm = DataEncryptionAlgorithm.AeadAes256CbcHmacSha256.ToString(),
				EncryptionType = EncryptionType.Deterministic,   // Less secure than randomized, but queryable
			};

			// Create the container with the two defined encrypted properties, partitioned on ID
			await database.DefineContainer("employees", "/id")
				.WithClientEncryptionPolicy()
				.WithIncludedPath(path1)
				.WithIncludedPath(path2)
				.Attach()
				.CreateAsync(throughput: 400);

			var container = client.GetContainer("human-resources", "employees");

			Console.WriteLine("Created employees container with two encrypted properties defined");

			// Add two employees
			await container.CreateItemAsync(new
			{
				id = "123456",
				firstName = "Jane",
				lastName = "Smith",
				department = "Customer Service",
				salary = new
				{
					baseSalary = 51280,
					bonus = 1440
				},
				ssn = "123-45-6789"
			}, new PartitionKey("123456"));

			await container.CreateItemAsync(new
			{
				id = "654321",
				firstName = "John",
				lastName = "Andersen",
				department = "Supply Chain",
				salary = new
				{
					baseSalary = 47920,
					bonus = 1810
				},
				ssn = "987-65-4321"
			}, new PartitionKey("654321"));

			Console.WriteLine("Created two employees; view encrypted properties in Data Explorer");
			Console.WriteLine("Press any key to continue");
			Console.ReadKey();
			Console.WriteLine();

			// Retrieve an employee via point read; SDK automatically decrypts Salary and SSN properties
			var employee = await container.ReadItemAsync<dynamic>("123456", new PartitionKey("123456"));
			Console.WriteLine("Retrieved employee via point read");
			Console.WriteLine(JsonConvert.SerializeObject(employee.Resource, Formatting.Indented));
			Console.WriteLine("Press any key to continue");
			Console.ReadKey();
			Console.WriteLine();

			// Retrieve an employee via SQL query; SDK automatically encrypts the SSN parameter value
			var queryDefinition = container.CreateQueryDefinition("SELECT * FROM c where c.ssn = @SSN");
			await queryDefinition.AddParameterAsync("@SSN", "987-65-4321", "/ssn");

			var results = await container.GetItemQueryIterator<dynamic>(queryDefinition).ReadNextAsync();
			Console.WriteLine("Retrieved employee via query");
			Console.WriteLine(JsonConvert.SerializeObject(results.First(), Formatting.Indented));
			Console.WriteLine("Press any key to continue");
			Console.ReadKey();
			Console.WriteLine();
		}

	}
}
