using MultiTool.Core;
using MultiTool.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;
using System.Collections;

namespace MultiTool.Utilities
{
	internal static class DatabaseUtilities
	{
		private static List<Vehicle> vehiclesCache = new List<Vehicle>();
		private static List<Item> itemsCache = new List<Item>();
		private static List<POI> POIsCache = new List<POI>();

		/// <summary>
		/// Load all vehicles and generate thumbnails
		/// </summary>
		/// <returns>List of vehicles</returns>
		internal static List<Vehicle> LoadVehicles()
		{
			// Return vehicles from cache if not empty.
			if (vehiclesCache.Count > 0)
				return vehiclesCache;

			// Cache empty, populate it.
			foreach (GameObject gameObject in itemdatabase.d.items)
			{
				try
				{
					if (GameUtilities.IsVehicleOrTrailer(gameObject))
					{
						// Check for variants.
						randomTypeSelector randoms = gameObject.GetComponent<randomTypeSelector>();
						if (randoms != null && randoms.tipusok.Length > 0)
						{
							int variants = randoms.tipusok.Length;

							for (int i = 0; i < variants; i++)
							{
								Vehicle vehicle = new Vehicle()
								{
									vehicle = gameObject,
									variant = i + 1,
									thumbnail = ThumbnailGenerator.GetThumbnail(gameObject, i + 2), // I have no idea why +1 produces the wrong variant in the thumbnail.
									name = Translator.T(gameObject.name, "vehicle", i + 1),
								};
								vehiclesCache.Add(vehicle);
							}
						}
						else
						{
							Vehicle vehicle = new Vehicle()
							{
								vehicle = gameObject,
								variant = -1,
								thumbnail = ThumbnailGenerator.GetThumbnail(gameObject),
								name = Translator.T(gameObject.name, "vehicle", -1),
							};
							vehiclesCache.Add(vehicle);
						}
					}
				}
				catch
				{
					Logger.Log($"Something went wrong loading vehicle {gameObject.name}", Logger.LogLevel.Error);
				}
			}

			return vehiclesCache;
		}

		/// <summary>
		/// Load items from database.
		/// </summary>
		/// <returns>List of items</returns>
		internal static List<Item> LoadItems()
		{
			// Return items from cache if not empty.
			if (itemsCache.Count > 0)
				return itemsCache;

			foreach (GameObject item in itemdatabase.d.items)
			{
				try
				{
					// Remove vehicles and trailers from items array.
					if (item && !GameUtilities.IsVehicleOrTrailer(item) && item.name != null && item.name != "ErrorPrefab")
					{
						itemsCache.Add(new Item() { item = item, thumbnail = ThumbnailGenerator.GetThumbnail(item), category = GameUtilities.GetCategory(item) });
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to load item {item.name} - {ex}", Logger.LogLevel.Error);
				}
			}

            // Load AMT database.
            Mod amt = ModLoader.LoadedMods.Where(m => m.ID == "AdvancedModdingToolkit").FirstOrDefault();
            if (amt != null)
            {
                int category = GUIRenderer.categories.Keys.ToList().IndexOf("Mod items");
                try
                {
                    Assembly amtAssembly = amt.GetType().Assembly;
                    Type database = amtAssembly.GetType("Amt.Database");
                    Type modItem = amtAssembly.GetType("Amt.ModItem");
                    PropertyInfo instanceProp = database.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                    object instance = instanceProp.GetValue(database, null);
                    var items = instance.GetType().GetProperty("Items", BindingFlags.Instance | BindingFlags.Public).GetValue(instance, null) as IEnumerable;
                    foreach (var item in items)
                    {
                        string key = item.GetType().GetProperty("Key").GetValue(item, null) as string;
                        object value = item.GetType().GetProperty("Value").GetValue(item, null);
                        MethodInfo spawn = value.GetType().GetMethod("Spawn", BindingFlags.Instance | BindingFlags.NonPublic);
                        GameObject gameObject = value.GetType().GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public).GetValue(value, null) as GameObject;

                        itemsCache.Add(new Item() { item = gameObject, thumbnail = ThumbnailGenerator.GetThumbnail(gameObject), category = category, amtItem = true, amtModItem = value, amtSpawn = spawn });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error occurred loading AMT items. Details: {ex}", Logger.LogLevel.Error);
                }
            }

            return itemsCache;
		}

		/// <summary>
		/// Load POIs from database.
		/// </summary>
		/// <returns>List of POIs</returns>
		internal static List<POI> LoadPOIs()
		{
			// Return cache if not empty.
			if (POIsCache.Count > 0)
				return POIsCache;

			// Cache empty, populate it.
			foreach (GameObject POI in itemdatabase.d.buildings)
			{
				if (POI.name == "ErrorPrefab" || POI.name == "Falu01") continue;

				try
				{
					// TODO: Some building thumbnails are a bit fucked.
					POIsCache.Add(new POI()
					{
						poi = POI,
						thumbnail = ThumbnailGenerator.GetThumbnail(POI, POI: true),
						name = Translator.T(POI.name, "POI"),
					});
				}
				catch (Exception ex)
				{
					Logger.Log($"POI init error - {ex}", Logger.LogLevel.Error);
				}
			}

			// Foliage objects.
			foreach (ObjClass objClass in mainscript.M.terrainGenerationSettings.objGeneration.objTypes)
			{
				POIsCache.Add(new POI()
				{
					poi = objClass.prefab,
					thumbnail = ThumbnailGenerator.GetThumbnail(objClass.prefab, POI: true),
					name = Translator.T(objClass.prefab.name, "POI"),
				});
			}

			// Desert tower buildings (ship, water tower, etc).
			foreach (ObjClass objClass in mainscript.M.terrainGenerationSettings.desertTowerGeneration.objTypes)
			{
				// Exclude POIs already loaded.
				if (POIsCache.Where(p => p.poi.name == objClass.prefab.name).ToList().Count() > 0) continue;

				POIsCache.Add(new POI()
				{
					poi = objClass.prefab,
					thumbnail = ThumbnailGenerator.GetThumbnail(objClass.prefab, POI: true),
					name = Translator.T(objClass.prefab.name, "POI"),
				});
			}

			return POIsCache;
		}

		/// <summary>
		/// Clear database caches.
		/// </summary>
		internal static void ClearCaches()
		{
			vehiclesCache.Clear();
			itemsCache.Clear();
			POIsCache.Clear();
		}

		/// <summary>
		/// Rebuild database caches.
		/// </summary>
		internal static void RebuildCaches()
		{
			ClearCaches();
			LoadVehicles();
			LoadItems();
			LoadPOIs();
		}
	}
}
