using MultiTool.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLDLoader;

namespace MultiTool.Utilities
{
	/// <summary>
	/// Migrate utilities from SpawnerTLD to MultiTool.
	/// </summary>
	internal static class MigrateUtilities
	{
		/// <summary>
		/// Migrate SpawnerTLD config to M-ultiTool.
		/// </summary>
		public static void MigrateFromSpawner()
		{
			string spawnerSettings = Path.Combine(ModLoader.ModsFolder, "Config", "Mod Settings", "SpawnerTLD");
			string path = ModLoader.GetModConfigFolder(MultiTool.mod);
			if (File.Exists(Path.Combine(spawnerSettings, "Config.json")))
			{
				// Delete existing config if it exists.
				if (File.Exists(Path.Combine(path, "Config.json")))
				{
					try
					{
						File.Delete(Path.Combine(path, "Config.json"));
					}
					catch (Exception ex)
					{
						Logger.Log($"Error removing M-ultiTool Config.json - {ex}", Logger.LogLevel.Error);
						return;
					}
				}

				try
				{
					File.Move(Path.Combine(spawnerSettings, "Config.json"), Path.Combine(path, "Config.json"));
					Logger.Log("Successfully migrated config from SpawnerTLD to M-ultiTool.");
				}
				catch (Exception ex)
				{
					Logger.Log($"Error migrating config from SpawnerTLD to M-ultiTool - {ex}", Logger.LogLevel.Error);
				}
			}
		}
	}
}
