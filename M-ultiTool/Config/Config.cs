using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Config
{
	internal class Configuration
	{
		public ConfigSerializable Config { get; private set; }

		private string _configPath = string.Empty;

		/// <summary>
		/// Load the config from the config file.
		/// </summary>
		public void Bootstrap(string path)
		{
			_configPath = path;

			try
			{
				if (File.Exists(_configPath))
				{
					string json = File.ReadAllText(_configPath);
					// Migrate from old to new configuration.
					if (json.Contains("version") && !json.Contains("Version"))
					{
						MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
						DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ConfigSerializableLegacy));
						var old = jsonSerializer.ReadObject(ms) as ConfigSerializableLegacy;
						ms.Close();

						Config = new ConfigSerializable
						{
							Keybinds = old.Keybinds,
							ScrollWidth = old.ScrollWidth != 0
								? old.ScrollWidth
								: 10f,
							Accessibility = old.Accessibility,
							AccessibilityModeAffectsColor = old.AccessibilityModeAffectsColor ?? true,
							NoclipFastMoveFactor = old.NoclipFastMoveFactor != 0
								? old.NoclipFastMoveFactor
								: 10f,
							Palette = old.Palette,
							BasicColliderColor = old.BasicColliderColor ?? new Color(1f, 0.0f, 0.0f, 0.8f),
							TriggerColliderColor = old.TriggerColliderColor ?? new Color(0.0f, 1f, 0.0f, 0.8f),
							InteriorColliderColor = old.InteriorColliderColor ?? new Color(0f, 0f, 1f, 0.8f),
							Theme = old.Theme
						};

						Logger.Log("Migrated configuration successfully.");
					}
					// Normal load flow.
					else
					{
						Config = JsonConvert.DeserializeObject<ConfigSerializable>(json);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Error loading config file: {ex}", Logger.LogLevel.Error);
				Config = new ConfigSerializable();
			}
		}

		/// <summary>
		/// Applies changes to the current configuration and immediately persists them to disk.
		/// </summary>
		/// <param name="mutator">
		/// A delegate that modifies the current <see cref="ConfigSerializable"/> instance.
		/// </param>
		public void Update(Action<ConfigSerializable> mutator)
		{
			mutator(Config);
			Commit();
		}

		/// <summary>
		/// Write the config to the file
		/// </summary>
		private void Commit()
		{
			Logger.Log("[Config] Commit called");
			if (_configPath == string.Empty)
			{
				Logger.Log("Config path not found", Logger.LogLevel.Error);
				return;
			}

			try
			{
				using (var file = File.CreateText(_configPath))
				{
					var serializer = new JsonSerializer
					{
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					};
					serializer.Serialize(file, Config);
				}

			}
			catch (Exception ex)
			{
				Logger.Log($"Config write error: {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
