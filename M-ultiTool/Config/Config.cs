using MultiTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Config
{
	internal class Configuration
	{
		private ConfigSerializable _config = new ConfigSerializable();
		private string _configPath = string.Empty;

		/// <summary>
		/// Load the config from the config file.
		/// </summary>
		private void loadFromConfigFile()
		{
			// Attempt to load the config file.
			try
			{
				// Config already loaded, return early.
				if (_config == new ConfigSerializable()) return;

				if (File.Exists(_configPath))
				{
					string json = File.ReadAllText(_configPath);
					MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ConfigSerializable));
					_config = jsonSerializer.ReadObject(ms) as ConfigSerializable;
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
			_configPath = path;
			loadFromConfigFile();
		}

		/// <summary>
		/// Update config version.
		/// </summary>
		public void UpdateVersion()
		{
			_config.Version = MultiTool.ModInstance.Version;
			Commit();
		}

		/// <summary>
		/// Update the config file keybinds
		/// </summary>
		/// <param name="binds">The new keybinds</param>
		public void UpdateKeybinds(List<Keybinds.Key> binds)
		{
			_config.Keybinds = binds;
			Commit();
		}

		/// <summary>
		/// Update scrollWidth in config
		/// </summary>
		/// <param name="width">The new scrollbar width</param>
		public void UpdateScrollWidth(float width)
		{
			_config.ScrollWidth = width;
			Commit();
		}

		/// <summary>
		/// Update accessibilityMode in config
		/// </summary>
		/// <param name="mode">The accessibility mode to set</param>
		public void UpdateAccessibilityMode(int mode)
		{
			_config.Accessibility = mode;
			Commit();
		}

		/// <summary>
		/// Update accessibilityModeAffectsColor in config
		/// </summary>
		/// <param name="accessibilityModeAffectsColor">Whether accessibility mode affects color labels</param>
		public void UpdateAccessibilityModeAffectsColor(bool accessibilityModeAffectsColor)
		{
			_config.AccessibilityModeAffectsColor = accessibilityModeAffectsColor;
			Commit();
		}

		/// <summary>
		/// Update noclipFastMoveFactor in config
		/// </summary>
		/// <param name="factor">The new factor</param>
		public void UpdateNoclipFastMoveFactor(float factor)
		{
			_config.NoclipFastMoveFactor = factor;
			Commit();
		}

		/// <summary>
		/// Update colour palette in config.
		/// </summary>
		/// <param name="palette">New palette</param>
		public void UpdatePalette(List<Color> palette)
		{
			_config.Palette = palette;
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
					_config.BasicColliderColor = color;
					break;
				case "trigger":
					_config.TriggerColliderColor = color;
					break;
				case "interior":
					_config.InteriorColliderColor = color;
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
			_config.Theme = theme;
			Commit();
		}

		/// <summary>
		/// Get config version.
		/// </summary>
		/// <returns></returns>
		public string GetVersion()
		{
			loadFromConfigFile();

			return _config.Version;
		}

		/// <summary>
		/// Get keybinds from the config file
		/// </summary>
		/// <returns>A list of keys</returns>
		public List<Keybinds.Key> GetKeybinds(List<Keybinds.Key> defaultBinds)
		{
			loadFromConfigFile();

			if (_config.Keybinds == null || _config.Keybinds.Count == 0)
				// No keybinds in config, write the defaults.
				UpdateKeybinds(defaultBinds);
			else if (_config.Keybinds.Count < defaultBinds.Count)
			{
				// Config is missing binds, update missing ones with defaults.
				List<Keybinds.Key> missing = defaultBinds.Where(k => !_config.Keybinds.Any(x => x.Action == k.Action)).ToList();
				foreach (Keybinds.Key key in missing)
				{
					_config.Keybinds.Add(key);
				}
				UpdateKeybinds(_config.Keybinds);
			}

			return _config.Keybinds;
		}

		/// <summary>
		/// Get scrollbar width from config
		/// </summary>
		/// <returns>The scrollbar width</returns>
		public float GetScrollWidth(float defaultScrollWidth)
		{
			loadFromConfigFile();

			if (_config.ScrollWidth == 0)
			{
				UpdateScrollWidth(defaultScrollWidth);
			}

			return _config.ScrollWidth;
		}

		/// <summary>
		/// Get accessibility mode from config
		/// </summary>
		/// <returns>Accessibility mode</returns>
		public int GetAccessibilityMode()
		{
			loadFromConfigFile();

			return _config.Accessibility;
		}

		/// <summary>
		/// Get accessibility mode affects color labels value from config
		/// </summary>
		/// <returns>Boolean, whether accessibility mode affects colour sliders</returns>
		public bool GetAccessibilityModeAffectsColor(bool defaultAccessibilityModeAffectsColor)
		{
			loadFromConfigFile();

			// Populate from default if not set in config.
			if (_config.AccessibilityModeAffectsColor == null)
			{
				UpdateAccessibilityModeAffectsColor(defaultAccessibilityModeAffectsColor);
			}

			return _config.AccessibilityModeAffectsColor.GetValueOrDefault();
		}

		/// <summary>
		/// Get noclip speed factor from config.
		/// </summary>
		/// <returns>Noclip speed factor</returns>
		public float GetNoclipFastMoveFactor(float defaultFactor)
		{
			loadFromConfigFile();

			if (_config.NoclipFastMoveFactor == 0)
			{
				UpdateNoclipFastMoveFactor(defaultFactor);
			}

			return _config.NoclipFastMoveFactor;
		}

		/// <summary>
		/// Get palette from config.
		/// </summary>
		/// <param name="defaultPalette">Default colour palette</param>
		/// <returns>Colour palette</returns>
		public List<Color> GetPalette(List<Color> defaultPalette)
		{
			loadFromConfigFile();

			if (_config.Palette == null || _config.Palette.Count == 0)
				// No palette, set default.
				_config.Palette = defaultPalette;

			return _config.Palette;
		}

		/// <summary>
		/// Get collider colour from config.
		/// </summary>
		/// <param name="colliderType">Collider type</param>
		/// <returns>Color for that collider type or white if it doesn't exist</returns>
		public Color GetColliderColour(string colliderType)
		{
			loadFromConfigFile();

			if (_config.BasicColliderColor == null) _config.BasicColliderColor = new Color(1f, 0.0f, 0.0f, 0.8f);
			if (_config.TriggerColliderColor == null) _config.TriggerColliderColor = new Color(0.0f, 1f, 0.0f, 0.8f);
			if (_config.InteriorColliderColor == null) _config.InteriorColliderColor = new Color(0f, 0f, 1f, 0.8f);

			switch (colliderType)
			{
				case "basic":
					return _config.BasicColliderColor.Value;
				case "trigger":
					return _config.TriggerColliderColor.Value;
				case "interior":
					return _config.InteriorColliderColor.Value;
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

			if (_config.Theme == null)
				_config.Theme = name;

			return _config.Theme;
		}

		/// <summary>
		/// Write the config to the file
		/// </summary>
		private void Commit()
		{
			if (_configPath == string.Empty)
			{
				Logger.Log("Config path not found", Logger.LogLevel.Error);
				return;
			}

			try
			{
				MemoryStream ms = new MemoryStream();
				DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ConfigSerializable));
				jsonSerializer.WriteObject(ms, _config);
				using (FileStream file = new FileStream(_configPath, FileMode.Create, FileAccess.Write))
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
