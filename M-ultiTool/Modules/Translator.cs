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
	internal static class Translator
	{
		// Translation-related variables.
		private static string _language;
		private static Dictionary<string, Translate> _translations = new Dictionary<string, Translate>();
		private static string _translationDir;

		public static void Init()
		{
			DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(ModLoader.GetModConfigFolder(MultiTool.mod), "Translations"));
			_translationDir = dir.FullName;

			LoadTranslationFiles();
		}

		/// <summary>
		/// Set translator language
		/// </summary>
		/// <param name="_language">The language to set the translator to</param>
		public static void SetLanguage(string language)
		{
			_language = language;
		}

		/// <summary>
		/// Load translation JSON files from mod config folder.
		/// </summary>
		private static void LoadTranslationFiles()
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
					MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Translate));
					var config = jsonSerializer.ReadObject(ms) as Translate;
					ms.Close();

					_translations.Add(Path.GetFileNameWithoutExtension(file), config);
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
            string defaultObjectName = objectName;

			// Fallback to English if the current language isn't supported.
			if (!_translations.ContainsKey(_language))
			{
				_language = "English";
			}

			if (_translations.ContainsKey(_language))
			{
                Translate config = _translations[_language];
                List<Translatable> translate = null;

                // Find translation list for type.
				switch (type)
				{
					case "vehicle":
                        translate = config.vehicles;
						break;
					case "POI":
						translate = config.POIs;
						break;
					case "menuVehicles":
						translate = config.menuVehicles;
						break;
				}

                // Attempt to find the translation.
                if (translate != null)
                {
                    foreach (Translatable translatable in translate)
                    {
                        if (translatable.objectName == objectName)
                        {
                            if (variant != null && variant != -1)
                            {
                                if (translatable.variant == variant)
                                {
                                    objectName = translatable.name;
                                    break;
                                }
                            }
                            else
                            {
                                objectName = translatable.name;
                                break;
                            }
                        }
                    }
                }
			}

            // No translation and has a variant, just append the variant number.
            if (defaultObjectName == objectName && variant != null && variant != -1)
			{
				objectName += $" (Variant {variant.GetValueOrDefault()})";
			}
			return objectName;
		}
	}
}
