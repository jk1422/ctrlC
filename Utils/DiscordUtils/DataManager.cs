using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static ctrlC.Utils.DiscordUtils.DiscordConnector;
using Colossal.Json;
using Colossal.Logging;

namespace ctrlC.Utils.DiscordUtils
{
	internal static class DataManager
	{
		private static string storagePath = "./data";
		public static string Membership { get; internal set; }
		public static ILog log = LogManager.GetLogger($"{nameof(ctrlC)}.{nameof(DataManager)}").SetShowsErrorsInUI(false);
		internal static void SaveUserData(DiscordUser user)
		{
			try
			{
				// Ensure the storage directory exists
				if (!Directory.Exists(storagePath))
				{
					Directory.CreateDirectory(storagePath);
				}

				// Serialize the user data to JSON
				string json = JSON.Dump(user);

				// Define the file path
				string filePath = Path.Combine(storagePath, $"data.json");

				// Write the JSON data to the file
				File.WriteAllText(filePath, json);

				
			}
			catch (Exception ex)
			{
				log.Error($"Failed to save user data: {ex.Message}");
			}
		}
		internal static bool TryLoadUserData(out DiscordUser user)
		{
			user = null;
			try
			{
				// Define the file path
				string filePath = Path.Combine(storagePath, $"data.json");

				// Check if the file exists
				if (!File.Exists(filePath))
				{
					log.Error($"User data file not found.");
					return false;
				}

				// Read the JSON data from the file
				string json = File.ReadAllText(filePath);

				// Deserialize the JSON data to a DiscordUser object
				Variant variant = JSON.Load(json);
				JSON.MakeInto<DiscordUser>(variant, out user);

				
				return true;
			}
			catch (Exception ex)
			{
				log.Error($"Failed to load user data: {ex.Message}");
				return false;
			}
		}
	}
}

