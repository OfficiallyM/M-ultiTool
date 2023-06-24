using SpawnerTLD.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TLDLoader;

namespace SpawnerTLD.Modules
{
	internal class Translator
	{
		// Translation-related variables.
		private string language;
		private Dictionary<string, List<ConfigVehicle>> translations = new Dictionary<string, List<ConfigVehicle>>();
		private string configDirectory;
		private Logger logger;

		public Translator(Logger _logger, string _configDirectory)
		{
			logger = _logger;

			configDirectory = _configDirectory;

			LoadTranslationFiles();
		}

		/// <summary>
		/// Set translator language
		/// </summary>
		/// <param name="_language">The language to set the translator to</param>
		public void SetLanguage(string _language)
		{
			language = _language;
		}

		/// <summary>
		/// Load translation JSON files from mod config folder.
		/// </summary>
		private void LoadTranslationFiles()
		{
			// Return early if the config directory doesn't exist.
			if (!Directory.Exists(configDirectory))
			{
				logger.Log("Config folder is missing, nothing will be translated", Logger.LogLevel.Error);
				return;
			}

			string[] files = Directory.GetFiles(configDirectory, "*.json");
			foreach (string file in files)
			{
				if (!File.Exists(file))
				{
					continue;
				}

				try
				{
					string json = File.ReadAllText(file);
					MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ConfigWrapper));
					var config = jsonSerializer.ReadObject(ms) as ConfigWrapper;
					ms.Close();

					translations.Add(Path.GetFileNameWithoutExtension(file), config.vehicles);
				}
				catch (Exception ex)
				{
					logger.Log($"Failed loading translation file {Path.GetFileNameWithoutExtension(file)} - error:\n{ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objectName">The object name to translate</param>
		/// <param name="variant">The vehicle variant (optional)</param>
		/// <returns>Translated object name or untranslated name if no translation is found</returns>
		public string T(string objectName, int? variant = null)
		{
			// Fallback to English if the current language isn't supported.
			if (!translations.ContainsKey(language))
			{
				language = "English";
			}

			if (translations.ContainsKey(language))
			{
				List<ConfigVehicle> vehicles = translations[language];
				foreach (ConfigVehicle vehicle in vehicles)
				{
					if (vehicle.objectName == objectName)
					{
						if (variant != null && variant != -1)
						{
							if (vehicle.variant == variant)
								return vehicle.name;
						}
						else
							return vehicle.name;
					}
				}
			}

			if (variant != null && variant != -1)
			{
				objectName += $" (Variant {variant.GetValueOrDefault()})";
			}
			return objectName;
		}
	}

}
