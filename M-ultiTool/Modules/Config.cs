using MultiTool.Core;
using MultiTool.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Modules
{
	internal class Config
	{
		private ConfigSerializable config = new ConfigSerializable();
		private string configPath = String.Empty;

		/// <summary>
		/// Load the config from the config file.
		/// </summary>
		private void loadFromConfigFile()
		{
			// Attempt to load the config file.
			try
			{
				// Config already loaded, return early.
				if (config == new ConfigSerializable()) return;

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
				Logger.Log($"Error loading config file: {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Set the path of the config file
		/// </summary>
		/// <param name="path">The config file path</param>
		public void SetConfigPath(string path)
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

			Commit();
		}

		/// <summary>
		/// Toggle legacy UI in config
		/// </summary>
		/// <param name="enabled">Whether the legacy UI is enabled</param>
		public void UpdateLegacyMode(bool enabled)
		{
			config.legacyUI = enabled;

			Commit();	
		}

		/// <summary>
		/// Update scrollWidth in config
		/// </summary>
		/// <param name="width">The new scrollbar width</param>
		public void UpdateScrollWidth(float width)
		{
			config.scrollWidth = width;

			Commit();
		}

		/// <summary>
		/// Toggle noclip godmode disable in config
		/// </summary>
		/// <param name="enabled">Whether godmode is disabled when leaving noclip</param>
		public void UpdateNoclipGodmodeDisable(bool enabled)
		{
			config.noclipGodmodeDisable = enabled;

			Commit();
		}

		/// <summary>
		/// Update accessibilityMode in config
		/// </summary>
		/// <param name="mode">The accessibility mode to set</param>
		public void UpdateAccessibilityMode(string mode)
		{
			config.accessibilityMode = mode;

			Commit();
		}

		/// <summary>
		/// Update noclipFastMoveFactor in config
		/// </summary>
		/// <param name="factor">The new factor</param>
		public void UpdateNoclipFastMoveFactor(float factor)
		{
			config.noclipFastMoveFactor = factor;

			Commit();
		}

		/// <summary>
		/// Update colour palette in config.
		/// </summary>
		/// <param name="palette">New palette</param>
		public void UpdatePalette(List<Color> palette)
		{
			config.palette = palette;

			Commit();
		}

		/// <summary>
		/// Update player data in config.
		/// </summary>
		/// <param name="playerData">New player data</param>
		public void UpdatePlayerData(PlayerData playerData)
		{
			config.playerData = playerData;

			Commit();
		}

		/// <summary>
		/// Get keybinds from the config file
		/// </summary>
		/// <returns>A list of keys</returns>
		public List<Keybinds.Key> GetKeybinds(List<Keybinds.Key> defaultBinds)
		{
			loadFromConfigFile();

			if (config.keybinds == null || config.keybinds.Count == 0)
				// No keybinds in config, write the defaults.
				UpdateKeybinds(defaultBinds);
			else if (config.keybinds.Count < defaultBinds.Count)
			{
				// Config is missing binds, update missing ones with defaults.
				List<Keybinds.Key> missing = defaultBinds.Where(k => !config.keybinds.Any(x => x.action == k.action)).ToList();
				foreach (Keybinds.Key key in missing)
				{
					config.keybinds.Add(key);
				}
				UpdateKeybinds(config.keybinds);
			}

			return config.keybinds;
		}

		/// <summary>
		/// Get legacy mode status from config
		/// </summary>
		/// <returns>Boolean legacy mode value</returns>
		public bool GetLegacyMode(bool defaultLegacyMode)
		{
			loadFromConfigFile();

			// Populate from default if not set in config.
			if (config.legacyUI == null)
			{
				UpdateLegacyMode(defaultLegacyMode);
			}

			return config.legacyUI.GetValueOrDefault();
		}

		/// <summary>
		/// Get scrollbar width from config
		/// </summary>
		/// <returns>The scrollbar width</returns>
		public float GetScrollWidth(float defaultScrollWidth)
		{
			loadFromConfigFile();

			if (config.scrollWidth == 0)
			{
				UpdateScrollWidth(defaultScrollWidth);
			}

			return config.scrollWidth;
		}

		/// <summary>
		/// Get noclip godmode disable status from config
		/// </summary>
		/// <returns>Boolean legacy mode value</returns>
		public bool GetNoclipGodmodeDisable(bool defaultNoclipGodmodeDisable)
		{
			loadFromConfigFile();

			if (config.noclipGodmodeDisable == null)
			{
				UpdateNoclipGodmodeDisable(defaultNoclipGodmodeDisable);
			}

			return config.noclipGodmodeDisable.GetValueOrDefault();
		}

		/// <summary>
		/// Get accessibility mode from config
		/// </summary>
		/// <returns>Boolean legacy mode value</returns>
		public string GetAccessibilityMode(string defaultAccessibilityMode)
		{
			loadFromConfigFile();

			if (config.accessibilityMode == null)
			{
				UpdateAccessibilityMode(defaultAccessibilityMode);
			}

			return config.accessibilityMode;
		}

		/// <summary>
		/// Get scrollbar width from config
		/// </summary>
		/// <returns>The scrollbar width</returns>
		public float GetNoclipFastMoveFactor(float defaultFactor)
		{
			loadFromConfigFile();

			if (config.noclipFastMoveFactor == 0)
			{
				UpdateNoclipFastMoveFactor(defaultFactor);
			}

			return config.noclipFastMoveFactor;
		}

		/// <summary>
		/// Get palette from config.
		/// </summary>
		/// <param name="defaultPalette">Default colour palette</param>
		/// <returns>Colour palette</returns>
		public List<Color> GetPalette(List<Color> defaultPalette)
		{
			loadFromConfigFile();

			if (config.palette == null || config.palette.Count == 0)
				// No palette, set default.
				config.palette = defaultPalette;

			return config.palette;
		}

		/// <summary>
		/// Get player data from config.
		/// </summary>
		/// <param name="defaultPlayerData">Default player data</param>
		/// <returns>Player data</returns>
		public PlayerData GetPlayerData(PlayerData defaultPlayerData)
		{
			loadFromConfigFile();

			if (config.playerData == null)
				config.playerData = defaultPlayerData.Copy();

			return config.playerData;
		}

		/// <summary>
		/// Write the config to the file
		/// </summary>
		private void Commit()
		{
			if (configPath == String.Empty)
			{
				Logger.Log("Config path not found", Logger.LogLevel.Error);
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
				Logger.Log($"Config write error: {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
