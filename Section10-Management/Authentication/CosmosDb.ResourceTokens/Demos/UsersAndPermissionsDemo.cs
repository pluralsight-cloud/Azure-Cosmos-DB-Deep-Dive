using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDb.ResourceTokens.Demos
{
	public static class UsersAndPermissionsDemo
	{
		private static Database _database;

		public async static Task Run()
		{
			Debugger.Break();

			_database = Shared.Client.GetDatabase("adventure-works");

			await ViewUsers();

			await CreateUser("Alice");
			await CreateUser("Tom");
			await ViewUsers();

			await ViewPermissions("Alice");
			await ViewPermissions("Tom");

			await CreatePermission("Alice", "StoresAllAccess", PermissionMode.All);
			await CreatePermission("Tom", "StoresReadonlyAccess", PermissionMode.Read);

			await ViewPermissions("Alice");
			await ViewPermissions("Tom");

			await TestPermissions("Alice");
			await TestPermissions("Tom");

			await DeletePermission("Alice", "StoresAllAccess");
			await DeletePermission("Tom", "StoresReadonlyAccess");

			await DeleteUser("Alice");
			await DeleteUser("Tom");
		}

		// Users

		private async static Task ViewUsers()
		{
			Console.WriteLine();
			Console.WriteLine($">>> View Users in {_database.Id} <<<");

			var iterator = _database.GetUserQueryIterator<UserProperties>();
			var usersCount = 0;
			while (iterator.HasMoreResults)
			{
				var usersProps = await iterator.ReadNextAsync();
				foreach (var userProps in usersProps)
				{
					usersCount++;
					Console.WriteLine($"User #{usersCount}: {userProps.Id}");
				}
			}

			Console.WriteLine();
			Console.WriteLine($"Total users in database {_database.Id}: {usersCount}");
		}

		private async static Task<UserProperties> CreateUser(string userId)
		{
			Console.WriteLine();
			Console.WriteLine($">>> Create User {userId} in {_database.Id} <<<");

			var result = await _database.CreateUserAsync(userId);
			var userProps = result.Resource;

			Console.WriteLine($"Created new user: {userId}");

			return userProps;
		}

		private async static Task DeleteUser(string userId)
		{
			Console.WriteLine();
			Console.WriteLine($">>> Delete User {userId} in {_database.Id} <<<");

			var user = _database.GetUser(userId);
			await user.DeleteAsync();

			Console.WriteLine($"Deleted user {userId} from database {_database.Id}");
		}

		// Permissions

		private async static Task ViewPermissions(string userId)
		{
			Console.WriteLine();
			Console.WriteLine($">>> View Permissions for {userId} <<<");

			var user = _database.GetUser(userId);
			var iterator = user.GetPermissionQueryIterator<PermissionProperties>();
			var permissionsCount = 0;
			while (iterator.HasMoreResults)
			{
				var permissionsProps = await iterator.ReadNextAsync();
				foreach (var permissionProps in permissionsProps)
				{
					permissionsCount++;
					Console.WriteLine($"Permission #{permissionsCount}: {permissionProps.Id} ({permissionProps.PermissionMode} on {permissionProps.ResourceUri})");
				}
			}

			Console.WriteLine();
			Console.WriteLine($"Total permissions for {userId}: {permissionsCount}");
		}

		private async static Task CreatePermission(string userId, string permissionId, PermissionMode permissionMode)
		{
			Console.WriteLine();
			Console.WriteLine($">>> Create Permission {permissionId} for {userId} <<<");

			var user = _database.GetUser(userId);
			var container = _database.GetContainer("stores");
			var permissionProps = new PermissionProperties(permissionId, permissionMode, container);
			await user.CreatePermissionAsync(permissionProps);

			Console.WriteLine($"Created new permission: {permissionId} ({permissionMode} on {permissionProps.ResourceUri})");
		}

		private async static Task DeletePermission(string userId, string permissionId)
		{
			Console.WriteLine();
			Console.WriteLine($">>> Delete Permission {permissionId} from {userId} <<<");

			var user = _database.GetUser(userId);
			var permission = user.GetPermission(permissionId);

			await permission.DeleteAsync();

			Console.WriteLine($"Deleted permission {permissionId} from user {user.Id}");
		}

		private async static Task TestPermissions(string userId)
		{
			dynamic doc = new
			{
				id = Guid.NewGuid().ToString(),
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

			var user = _database.GetUser(userId);
			var iterator = user.GetPermissionQueryIterator<PermissionProperties>();
			var permissionProps = (await iterator.ReadNextAsync()).First();

			var resourceToken = permissionProps.Token;

			Console.WriteLine();
			Console.WriteLine($"Trying to create & delete document as user {user.Id}");
			try
			{
				var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
				var endpoint = config["CosmosEndpoint"];
				using (var restrictedClient = new CosmosClient(endpoint, resourceToken))
				{
					var cont = restrictedClient.GetContainer("adventure-works", "stores");

					var document = await cont.CreateItemAsync(doc);
					Console.WriteLine($"Successfully created document: {doc.id}");

					await cont.DeleteItemAsync<dynamic>(doc.id.ToString(), new PartitionKey("11229"));
					Console.WriteLine($"Successfully deleted document: {doc.id}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR: {ex.Message}");
			}
		}

	}
}
