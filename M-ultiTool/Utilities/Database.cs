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
        private static Assembly amtAssembly;
        private static IEnumerable amtItems;
        private static bool hasAmtSetupRan = false;

        /// <summary>
        /// Initial AMT database parsing.
        /// </summary>
        /// <returns>True if loaded correctly, otherwise false</returns>
        private static bool AMTSetup()
        {
            if (amtItems != null || hasAmtSetupRan) return true;
            // Load AMT database.
            Mod amt = ModLoader.LoadedMods.Where(m => m.ID == "AdvancedModdingToolkit").FirstOrDefault();
            if (amt != null)
            {
                Version amtVersion = new Version(amt.Version);
                if (amtVersion.CompareTo(new Version("0.3.0.0")) >= 0)
                {
                    try
                    {
                        amtAssembly = amt.GetType().Assembly;
                        Type database = amtAssembly.GetType("Amt.Database");
                        Type modItem = amtAssembly.GetType("Amt.ModItem");
                        PropertyInfo instanceProp = database.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                        object instance = instanceProp.GetValue(database, null);
                        amtItems = instance.GetType().GetProperty("Items", BindingFlags.Instance | BindingFlags.Public).GetValue(instance, null) as IEnumerable;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error occurred loading AMT items. Details: {ex}", Logger.LogLevel.Error);
                    }
                }
                else
                    Logger.Log("Outdated AMT version, please update for it to support M-ultiTool.", Logger.LogLevel.Error);
            }

            hasAmtSetupRan = true;
            return false;
        }

        /// <summary>
        /// Load all AMT vehicles.
        /// </summary>
        /// <returns>List of vehicles</returns>
        private static List<Vehicle> LoadAMTVehicles()
        {
            List<Vehicle> amtVehicles = new List<Vehicle>();
            if (AMTSetup())
            {
                foreach (var item in amtItems)
                {
                    string key = item.GetType().GetProperty("Key").GetValue(item, null) as string;
                    try
                    {
                        object value = item.GetType().GetProperty("Value").GetValue(item, null);
                        MethodInfo spawn = value.GetType().GetMethod("ManualSpawn", BindingFlags.Instance | BindingFlags.Public);
                        GameObject gameObject = value.GetType().GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public).GetValue(value, null) as GameObject;

                        Type controllerType = amtAssembly.GetType("Amt.Vehicles.VehicleController");
                        var controller = gameObject.GetComponent(controllerType);
                        if (controller != null)
                        {
                            AMTData data = new AMTData()
                            {
                                modItem = value,
                                spawnMethod = spawn,
                            };

                            amtVehicles.Add(new Vehicle() { gameObject = gameObject, name = key, thumbnail = ThumbnailGenerator.GetThumbnail(gameObject), amt = data });
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error occurred loading AMT vehicle {key}. Details: {ex}", Logger.LogLevel.Error);
                    }
                }
            }

            return amtVehicles;
        }

        /// <summary>
        /// Load all AMT items.
        /// </summary>
        /// <returns>List of items</returns>
        private static List<Item> LoadAMTItems()
        {
            List<Item> amtItems = new List<Item>();
            if (AMTSetup())
            {
                int category = GUIRenderer.categories.Keys.ToList().IndexOf("Mod items");

                foreach (var item in amtItems)
                {
                    string key = item.GetType().GetProperty("Key").GetValue(item, null) as string;
                    try
                    {
                        object value = item.GetType().GetProperty("Value").GetValue(item, null);
                        MethodInfo spawn = value.GetType().GetMethod("ManualSpawn", BindingFlags.Instance | BindingFlags.Public);
                        GameObject gameObject = value.GetType().GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public).GetValue(value, null) as GameObject;

                        Type controllerType = amtAssembly.GetType("Amt.Vehicles.VehicleController");
                        var controller = gameObject.GetComponent(controllerType);
                        if (controller == null)
                        {
                            AMTData data = new AMTData()
                            {
                                modItem = value,
                                spawnMethod = spawn,
                            };

                            amtItems.Add(new Item() { gameObject = gameObject, thumbnail = ThumbnailGenerator.GetThumbnail(gameObject), amt = data, category = category });
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error occurred loading AMT item {key}. Details: {ex}", Logger.LogLevel.Error);
                    }
                }
            }

            return amtItems;
        }

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
									gameObject = gameObject,
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
								gameObject = gameObject,
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

            // Populate with AMT vehicles.
            vehiclesCache.AddRange(LoadAMTVehicles());

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
						itemsCache.Add(new Item() { gameObject = item, thumbnail = ThumbnailGenerator.GetThumbnail(item), category = GameUtilities.GetCategory(item) });
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to load item {item.name} - {ex}", Logger.LogLevel.Error);
				}
			}

            // Populate with AMT items.
            itemsCache.AddRange(LoadAMTItems());

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
