using MultiTool.Services;
using MultiTool.UI;
using MultiTool.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Data
{
	internal class Database
	{
		public List<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();
		public List<Item> Items { get; private set; } = new List<Item>();
		public List<Poi> Pois { get; private set; } = new List<Poi>();

		private Assembly _amtAssembly;
		private IEnumerable _amtItems;
		private bool _hasAmtSetupRan = false;

		public void FetchData()
		{
			Vehicles.Clear();
			Items.Clear();
			Pois.Clear();

			LoadVehicles();
			LoadItems();
			LoadPOIs();
		}

		/// <summary>
		/// Load vehicles from database and generate thumbnails.
		/// </summary>
		private void LoadVehicles()
		{
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
									GameObject = gameObject,
									//variant = i + 1,
									Variant = i,
									Thumbnail = ThumbnailGenerator.GetThumbnail(gameObject, i),
									Name = Translator.T(gameObject.name, "vehicle", i),
								};
								Vehicles.Add(vehicle);
							}
						}
						else
						{
							Vehicle vehicle = new Vehicle()
							{
								GameObject = gameObject,
								Variant = -1,
								Thumbnail = ThumbnailGenerator.GetThumbnail(gameObject),
								Name = Translator.T(gameObject.name, "vehicle", -1),
							};
							Vehicles.Add(vehicle);
						}
					}
				}
				catch
				{
					Logger.Log($"Something went wrong loading vehicle {gameObject.name}", Logger.LogLevel.Error);
				}
			}

			// Populate with AMT vehicles.
			Vehicles.AddRange(LoadAMTVehicles());
		}

		/// <summary>
		/// Load items from database and generate thumbnails.
		/// </summary>
		private void LoadItems()
		{
			foreach (GameObject item in itemdatabase.d.items)
			{
				try
				{
					// Remove vehicles and trailers from items array.
					if (item && !GameUtilities.IsVehicleOrTrailer(item) && item.name != null && item.name != "ErrorPrefab")
					{
						Items.Add(new Item() { GameObject = item, Thumbnail = ThumbnailGenerator.GetThumbnail(item), Category = GameUtilities.GetCategory(item) });
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to load item {item.name} - {ex}", Logger.LogLevel.Error);
				}
			}

			// Populate with AMT items.
			Items.AddRange(LoadAMTItems());

			// Populate with mod items.
			Items.AddRange(LoadModItems());
		}

		/// <summary>
		/// Load POIs from database.
		/// </summary>
		/// <returns>List of POIs</returns>
		private void LoadPOIs()
		{
			foreach (GameObject POI in itemdatabase.d.buildings)
			{
				if (POI.name == "ErrorPrefab" || POI.name == "Falu01") continue;

				try
				{
					// TODO: Some building thumbnails are a bit fucked.
					Pois.Add(new Poi()
					{
						Obj = POI,
						Thumbnail = ThumbnailGenerator.GetThumbnail(POI, POI: true),
						Name = Translator.T(POI.name, "POI"),
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
				Pois.Add(new Poi()
				{
					Obj = objClass.prefab,
					Thumbnail = ThumbnailGenerator.GetThumbnail(objClass.prefab, POI: true),
					Name = Translator.T(objClass.prefab.name, "POI"),
				});
			}

			// Desert tower buildings (ship, water tower, etc).
			foreach (ObjClass objClass in mainscript.M.terrainGenerationSettings.desertTowerGeneration.objTypes)
			{
				// Exclude POIs already loaded.
				if (Pois.Where(p => p.Obj.name == objClass.prefab.name).ToList().Count() > 0) continue;

				Pois.Add(new Poi()
				{
					Obj = objClass.prefab,
					Thumbnail = ThumbnailGenerator.GetThumbnail(objClass.prefab, POI: true),
					Name = Translator.T(objClass.prefab.name, "POI"),
				});
			}

			// Find unused buildings.
			string[] buildingNames = new string[]
			{
				"Pyramid1",
				"toalettarium",
				"ROADSIGNS1"
			};
			GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
			foreach (GameObject obj in allObjects)
			{
				if (buildingNames.Contains(obj.name) && obj.transform.parent != null && obj.transform.parent.name == "ForRunden")
				{
					Pois.Add(new Poi()
					{
						Obj = obj,
						Thumbnail = ThumbnailGenerator.GetThumbnail(obj, POI: true),
						Name = Translator.T(obj.name, "POI"),
					});
				}
			}
		}

		/// <summary>
		/// Initial AMT database parsing.
		/// </summary>
		/// <returns>True if loaded correctly, otherwise false</returns>
		private bool AMTSetup()
		{
			if (_amtItems != null) return true;

			if (_hasAmtSetupRan) return _amtItems != null;

			// Load AMT database.
			Mod amt = ModLoader.LoadedMods.Where(m => m.ID == "AdvancedModdingToolkit").FirstOrDefault();
			if (amt != null)
			{
				Version amtVersion = new Version(amt.Version);
				if (amtVersion.CompareTo(new Version("0.3.0.0")) >= 0)
				{
					try
					{
						_amtAssembly = amt.GetType().Assembly;
						Type database = _amtAssembly.GetType("Amt.Database");
						Type modItem = _amtAssembly.GetType("Amt.ModItem");
						PropertyInfo instanceProp = database.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
						object instance = instanceProp.GetValue(database, null);
						_amtItems = instance.GetType().GetProperty("Items", BindingFlags.Instance | BindingFlags.Public).GetValue(instance, null) as IEnumerable;
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

			_hasAmtSetupRan = true;
			return false;
		}

		/// <summary>
		/// Load all AMT vehicles.
		/// </summary>
		/// <returns>List of vehicles</returns>
		private List<Vehicle> LoadAMTVehicles()
		{
			List<Vehicle> amtVehicles = new List<Vehicle>();
			if (AMTSetup())
			{
				foreach (object item in _amtItems)
				{
					string key = item.GetType().GetProperty("Key").GetValue(item, null) as string;
					try
					{
						object value = item.GetType().GetProperty("Value").GetValue(item, null);
						MethodInfo spawn = value.GetType().GetMethod("ManualSpawn", BindingFlags.Instance | BindingFlags.Public);
						GameObject gameObject = value.GetType().GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public).GetValue(value, null) as GameObject;

						Type controllerType = _amtAssembly.GetType("Amt.Vehicles.VehicleController");
						Component controller = gameObject.GetComponent(controllerType);
						if (controller != null)
						{
							AMTData data = new AMTData()
							{
								modItem = value,
								spawnMethod = spawn,
							};

							amtVehicles.Add(new Vehicle() { GameObject = gameObject, Name = key, Thumbnail = ThumbnailGenerator.GetThumbnail(gameObject), Amt = data });
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
		private List<Item> LoadAMTItems()
		{
			List<Item> items = new List<Item>();
			if (AMTSetup())
			{
				int category = GUIRenderer.Categories.Keys.ToList().IndexOf("Mod items");

				foreach (object item in _amtItems)
				{
					string key = item.GetType().GetProperty("Key").GetValue(item, null) as string;
					try
					{
						object value = item.GetType().GetProperty("Value").GetValue(item, null);
						MethodInfo spawn = value.GetType().GetMethod("ManualSpawn", BindingFlags.Instance | BindingFlags.Public);
						GameObject gameObject = value.GetType().GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public).GetValue(value, null) as GameObject;

						Type controllerType = _amtAssembly.GetType("Amt.Vehicles.VehicleController");
						Component controller = gameObject.GetComponent(controllerType);
						if (controller == null)
						{
							AMTData data = new AMTData()
							{
								modItem = value,
								spawnMethod = spawn,
							};

							items.Add(new Item() { GameObject = gameObject, Thumbnail = ThumbnailGenerator.GetThumbnail(gameObject), Amt = data, Category = category });
						}
					}
					catch (Exception ex)
					{
						Logger.Log($"Error occurred loading AMT item {key}. Details: {ex}", Logger.LogLevel.Error);
					}
				}
			}

			return items;
		}

		private List<Item> LoadModItems()
		{
			List<Item> items = new List<Item>();
			int category = GUIRenderer.Categories.Keys.ToList().IndexOf("Mod items");

			foreach (GameObject item in ModLoader.Database.GetAllItems())
			{
				try
				{
					items.Add(new Item() { GameObject = item, Thumbnail = ThumbnailGenerator.GetThumbnail(item), Category = category });
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to load mod item {(item?.name ?? "Unknown")} - {ex}", Logger.LogLevel.Error);
				}
			}

			return items;
		}
	}
}
