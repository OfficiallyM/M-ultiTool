using MultiTool.Core;
using MultiTool.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization.Json;
using System.Text;
using TLDLoader;
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
        /// Update config version.
        /// </summary>
        public void UpdateVersion()
        {
            config.version = MultiTool.mod.Version;
            Commit();
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
		/// Update scrollWidth in config
		/// </summary>
		/// <param name="width">The new scrollbar width</param>
		public void UpdateScrollWidth(float width)
		{
			config.scrollWidth = width;
			Commit();
		}

		/// <summary>
		/// Update accessibilityMode in config
		/// </summary>
		/// <param name="mode">The accessibility mode to set</param>
		public void UpdateAccessibilityMode(int mode)
		{
			config.accessibility = mode;
			Commit();
		}

		/// <summary>
		/// Update accessibilityModeAffectsColor in config
		/// </summary>
		/// <param name="accessibilityModeAffectsColor">Whether accessibility mode affects color labels</param>
		public void UpdateAccessibilityModeAffectsColor(bool accessibilityModeAffectsColor)
		{
			config.accessibilityModeAffectsColor = accessibilityModeAffectsColor;
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
        /// Get collider colour from config.
        /// </summary>
        /// <param name="color">New color</param>
        /// <param name="colliderType">Collider type</param>
        public void UpdateColliderColour(Color color, string colliderType)
        {
            switch (colliderType)
            {
                case "basic":
                    config.basicColliderColor = color;
                    break;
                case "trigger":
                    config.triggerColliderColor = color;
                    break;
                case "interior":
                    config.interiorColliderColor = color;
                    break;
            }
            Commit();
        }

		/// <summary>
		/// Update active theme.
		/// </summary>
		/// <param name="theme">New active theme name</param>
		public void UpdateTheme(string theme)
		{
			config.theme = theme;
			Commit();
		}

        /// <summary>
        /// Get config version.
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            loadFromConfigFile();

            return config.version;
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
		/// Get accessibility mode from config
		/// </summary>
		/// <returns>Accessibility mode</returns>
		public int GetAccessibilityMode()
		{
			loadFromConfigFile();

			return config.accessibility;
		}

		/// <summary>
		/// Get accessibility mode affects color labels value from config
		/// </summary>
		/// <returns>Boolean, whether accessibility mode affects colour sliders</returns>
		public bool GetAccessibilityModeAffectsColor(bool defaultAccessibilityModeAffectsColor)
		{
			loadFromConfigFile();

			// Populate from default if not set in config.
			if (config.accessibilityModeAffectsColor == null)
			{
				UpdateAccessibilityModeAffectsColor(defaultAccessibilityModeAffectsColor);
			}

			return config.accessibilityModeAffectsColor.GetValueOrDefault();
		}

		/// <summary>
		/// Get noclip speed factor from config.
		/// </summary>
		/// <returns>Noclip speed factor</returns>
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
        /// Get collider colour from config.
        /// </summary>
        /// <param name="colliderType">Collider type</param>
        /// <returns>Color for that collider type or white if it doesn't exist</returns>
        public Color GetColliderColour(string colliderType)
        {
            loadFromConfigFile();

            if (config.basicColliderColor == null) config.basicColliderColor = new Color(1f, 0.0f, 0.0f, 0.8f);
            if (config.triggerColliderColor == null) config.triggerColliderColor = new Color(0.0f, 1f, 0.0f, 0.8f);
            if (config.interiorColliderColor == null) config.interiorColliderColor = new Color(0f, 0f, 1f, 0.8f);

            switch (colliderType)
            {
                case "basic":
                    return config.basicColliderColor.Value;
                case "trigger":
                    return config.triggerColliderColor.Value;
                case "interior":
                    return config.interiorColliderColor.Value;
            }

            return Color.white;
        }

		/// <summary>
		/// Get active theme name.
		/// </summary>
		/// <param name="name">Default theme name</param>
		/// <returns>Active theme name</returns>
		public string GetTheme(string name)
		{
			loadFromConfigFile();

			if (config.theme == null)
				config.theme = name;

			return config.theme;
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
