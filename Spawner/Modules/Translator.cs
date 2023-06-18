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
		private Mod mod = null;
		private Logger logger;

		public Translator(string _language, Mod _mod)
		{
			language = _language;
			mod = _mod;

			logger = new Logger();

			LoadTranslationFiles();
		}

		/// <summary>
		/// Load translation JSON files from mod config folder.
		/// </summary>
		private void LoadTranslationFiles()
		{
			// Return early if the config directory doesn't exist.
			if (!Directory.Exists(ModLoader.GetModConfigFolder(mod)))
			{
				logger.Log("Config folder is missing, nothing will be translated", Logger.LogLevel.Error);
				return;
			}

			string[] files = Directory.GetFiles(ModLoader.GetModConfigFolder(mod), "*.json");
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
