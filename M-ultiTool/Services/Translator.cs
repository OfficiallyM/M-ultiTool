using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TLDLoader;

namespace MultiTool.Services
{
	internal class Translator
	{
		private string _language;
		private Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>();
		private string _translationDir;

		public Translator()
		{
			DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(ModLoader.GetModConfigFolder(MultiTool.ModInstance), "Translations"));
			_translationDir = dir.FullName;

			LoadTranslationFiles();
		}

		/// <summary>
		/// Set translator language.
		/// </summary>
		/// <param name="language">The language to set the translator to</param>
		public void SetLanguage(string language)
		{
			_language = language;
		}

		/// <summary>
		/// Translate a key into the current language.
		/// </summary>
		/// <param name="key">The translation key to look up</param>
		/// <param name="defaultValue">The value to return if the key has no translation</param>
		/// <returns>Translated value, or defaultValue if no translation is found</returns>
		public string T(string key, string defaultValue)
		{
			// Fallback to English if the current language isn't supported.
			if (!_translations.ContainsKey(_language))
				_language = "English";

			return _translations.ContainsKey(_language) && _translations[_language].TryGetValue(key, out string translated)
				? translated
				: defaultValue;
		}

		/// <summary>
		/// Load translation JSON files from mod config folder.
		/// </summary>
		private void LoadTranslationFiles()
		{
			// Return early if translations are already loaded.
			if (_translations.Count > 0)
				return;

			string[] files = Directory.GetFiles(_translationDir, "*.json");
			foreach (string file in files)
			{
				if (!File.Exists(file))
				{
					continue;
				}

				try
				{
					string json = File.ReadAllText(file);
					Dictionary<string, string> translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

					_translations.Add(Path.GetFileNameWithoutExtension(file), translations);
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed loading translation file {Path.GetFileNameWithoutExtension(file)} - error:\n{ex}", Logger.LogLevel.Error);
				}
			}
		}
	}
}
