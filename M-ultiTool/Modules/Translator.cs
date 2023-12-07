using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TLDLoader;

namespace MultiTool.Modules
{
	public static class Translator
	{
		// Translation-related variables.
		private static string language;
		private static Dictionary<string, ConfigWrapper> translations = new Dictionary<string, ConfigWrapper>();
		private static string configDirectory;

		public static void Init()
		{
			configDirectory = Path.Combine(ModLoader.GetModConfigFolder(MultiTool.mod), "Translations");
			LoadTranslationFiles();
		}

		/// <summary>
		/// Set translator language
		/// </summary>
		/// <param name="_language">The language to set the translator to</param>
		public static void SetLanguage(string _language)
		{
			language = _language;
		}

		/// <summary>
		/// Load translation JSON files from mod config folder.
		/// </summary>
		private static void LoadTranslationFiles()
		{
			// Return early if the config directory doesn't exist.
			if (!Directory.Exists(configDirectory))
			{
				Logger.Log("Config folder is missing, nothing will be translated", Logger.LogLevel.Error);
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

					translations.Add(Path.GetFileNameWithoutExtension(file), config);
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed loading translation file {Path.GetFileNameWithoutExtension(file)} - error:\n{ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objectName">The object name to translate</param>
		/// <param name="variant">The vehicle variant (optional)</param>
		/// <returns>Translated object name or untranslated name if no translation is found</returns>
		public static string T(string objectName, string type, int? variant = null)
		{
			// Fallback to English if the current language isn't supported.
			if (!translations.ContainsKey(language))
			{
				language = "English";
			}

			if (translations.ContainsKey(language))
			{
				ConfigWrapper config = translations[language];
				switch (type)
				{
					case "vehicle":
						List<ConfigVehicle> vehicles = config.vehicles;
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
						break;
					case "POI":
						List<ConfigPOI> POIs = config.POIs;
						foreach (ConfigPOI POI in POIs)
						{
							if (POI.objectName == objectName)
							{
								return POI.name;
							}
						}
						break;
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
