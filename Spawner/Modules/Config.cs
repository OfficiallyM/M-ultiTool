using SpawnerTLD.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace SpawnerTLD.Modules
{
	internal class Config
	{
		private ConfigSerializable config = new ConfigSerializable();
		private string configPath = String.Empty;

		// Modules.
		private Logger logger;

		public Config(Logger _logger)
		{
			logger = _logger;
		}

		/// <summary>
		/// Load the config from the config file.
		/// </summary>
		private void loadFromConfigFile()
		{
			// Attempt to load the config file.
			try
			{
				if (File.Exists(configPath))
				{
					string json = File.ReadAllText(configPath);
					MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ConfigSerializable));
					config = jsonSerializer.ReadObject(ms) as ConfigSerializable;
					ms.Close();
				}
			}
			catch (Exception ex)
			{
				logger.Log($"Error loading config file: {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Set the path of the config file
		/// </summary>
		/// <param name="path">The config file path</param>
		public void setConfigPath(string path)
		{
			configPath = path;

			loadFromConfigFile();
		}

		/// <summary>
		/// Update the config file keybinds
		/// </summary>
		/// <param name="binds">The new keybinds</param>
		public void UpdateKeybinds(List<Keybinds.Key> binds)
		{
			config.keybinds = binds;

			UpdateConfig();
		}

		/// <summary>
		/// Toggle legacy UI in config
		/// </summary>
		/// <param name="enabled">Whether the legacy UI is enabled</param>
		public void UpdateLegacyMode(bool enabled)
		{
			config.legacyUI = enabled;

			UpdateConfig();	
		}

		/// <summary>
		/// Update scrollWidth in config
		/// </summary>
		/// <param name="width">The new scrollbar width</param>
		public void UpdateScrollWidth(float width)
		{
			config.scrollWidth = width;

			UpdateConfig();
		}

		/// <summary>
		/// Toggle noclip godmode disable in config
		/// </summary>
		/// <param name="enabled">Whether godmode is disabled when leaving noclip</param>
		public void UpdateNoclipGodmodeDisable(bool enabled)
		{
			config.noclipGodmodeDisable = enabled;

			UpdateConfig();
		}

		/// <summary>
		/// Get keybinds from the config file
		/// </summary>
		/// <returns>A list of keys</returns>
		public List<Keybinds.Key> GetKeybinds()
		{
			loadFromConfigFile();

			if (config.keybinds != null && config.keybinds.Count > 0)
			{
				return config.keybinds;
			}

			return null;
		}

		/// <summary>
		/// Get legacy mode status from config
		/// </summary>
		/// <returns>Boolean legacy mode value</returns>
		public bool? GetLegacyMode()
		{
			loadFromConfigFile();

			if (config != null && config.legacyUI != null)
			{
				return config.legacyUI;
			}

			return null;
		}

		/// <summary>
		/// Get scrollbar width from config
		/// </summary>
		/// <returns>The scrollbar width</returns>
		public float? GetScrollWidth()
		{
			loadFromConfigFile();

			if (config != null && config.scrollWidth > 0)
			{
				return config.scrollWidth;
			}

			return null;
		}

		/// <summary>
		/// Get legacy mode status from config
		/// </summary>
		/// <returns>Boolean legacy mode value</returns>
		public bool? GetNoclipGodmodeDisable()
		{
			loadFromConfigFile();

			if (config != null && config.noclipGodmodeDisable != null)
			{
				return config.noclipGodmodeDisable;
			}

			return null;
		}

		/// <summary>
		/// Write the config to the file
		/// </summary>
		private void UpdateConfig()
		{
			if (configPath == String.Empty)
			{
				logger.Log("Config path not found", Logger.LogLevel.Error);
				return;
			}

			try
			{
				MemoryStream ms = new MemoryStream();
				DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ConfigSerializable));
				jsonSerializer.WriteObject(ms, config);
				using (FileStream file = new FileStream(configPath, FileMode.Create, FileAccess.Write))
				{
					ms.WriteTo(file);
					ms.Dispose();
				}

			}
			catch (Exception ex)
			{
				logger.Log($"Config write error: {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
