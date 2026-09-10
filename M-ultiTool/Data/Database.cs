using MultiTool.Extensions;
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
		private ServiceContext _services;

		public List<Item> Vehicles { get; private set; } = new List<Item>();
		public List<Item> Items { get; private set; } = new List<Item>();
		public List<Poi> Pois { get; private set; } = new List<Poi>();

		private Assembly _amtAssembly;
		private IEnumerable _amtItems;
		private bool _hasAmtSetupRan = false;

		private Dictionary<string, List<Type>> _categories = new Dictionary<string, List<Type>>()
		{
			{ "Vehicle chassis", new List<Type>() { typeof(carscript) } },
			{ "Trailers", new List<Type>() { typeof(utanfutoscript) } },
			{ "Tanks", new List<Type>() { typeof(tankscript) } },
			{ "Lights", new List<Type>() { typeof(headlightscript) } },
			{ "Engines", new List<Type>() { typeof(enginescript) } },
			{ "Wheels", new List<Type>() { typeof(wheelscript) } },
			{ "Tires", new List<Type>() { typeof(gumiscript) } },
			{ "Dials", new List<Type>() { typeof(meterscript) } },
			{ "Attachables", new List<Type>() { typeof(attachablescript) } },
			{ "Other vehicle parts", new List<Type>() { typeof(attachablescript) } },
			{ "Guns", new List<Type>() { typeof(weaponscript) } },
			{ "Melee weapons", new List<Type>() { typeof(meleeweaponscript) } },
			{ "Cleaning", new List<Type>() { typeof(drotkefescript), typeof(spricniscript) } },
			{ "Refillables", new List<Type>() { typeof(ammoscript) } },
			{ "Food", new List<Type>() { typeof(ediblescript) } },
			{ "Wearables", new List<Type>() { typeof(wearable) } },
			{ "Usables", new List<Type>() { typeof(pickupable) } },
			{ "Mod items", new List<Type>() { typeof(tosaveitemscript) } },
			{ "Other", new List<Type>() { typeof(MonoBehaviour) } },
		};

		public Database(ServiceContext services)
		{
			_services = services;
		}

		public void FetchData()
		{
			Vehicles.Clear();
			Items.Clear();
			Pois.Clear();

			LoadVehicles();
			LoadItems();
			LoadPOIs();
		}

		public List<string> GetCategories()
			=> _categories.Keys.ToList();

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
								Item vehicle = new Item()
								{
									Name = _services.Translator.T($"vehicle.{gameObject.name.ToKey()}.{i}", gameObject.name),
									Variant = i,
									GameObject = gameObject,
									Thumbnail = ThumbnailGenerator.GetThumbnail(gameObject, i),
								};
								Vehicles.Add(vehicle);
							}
						}
						else
						{
							Item vehicle = new Item()
							{
								Name = _services.Translator.T($"vehicle.{gameObject.name.ToKey()}", gameObject.name),
								Variant = -1,
								GameObject = gameObject,
								Thumbnail = ThumbnailGenerator.GetThumbnail(gameObject),
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
						Items.Add(new Item() {
							Name = _services.Translator.T($"item.{item.name.ToKey()}", item.name),
							Category = GetCategory(item),
							GameObject = item,
							Thumbnail = ThumbnailGenerator.GetThumbnail(item),
						});
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
			foreach (GameObject building in itemdatabase.d.buildings)
			{
				if (building.name == "ErrorPrefab" || building.name == "Falu01") continue;

				try
				{
					// TODO: Some building thumbnails are a bit fucked.
					Pois.Add(new Poi()
					{
						Obj = building,
						Thumbnail = ThumbnailGenerator.GetThumbnail(building, POI: true),
						Name = _services.Translator.T($"poi.{building.name.ToKey()}", building.name),
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
					Name = _services.Translator.T($"poi.{objClass.prefab.name.ToKey()}", objClass.prefab.name),
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
					Name = _services.Translator.T($"poi.{objClass.prefab.name.ToKey()}", objClass.prefab.name),
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
						Name = _services.Translator.T($"poi.{obj.name.ToKey()}", obj.name),
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
		private List<Item> LoadAMTVehicles()
		{
			List<Item> amtVehicles = new List<Item>();
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

							amtVehicles.Add(new Item() {
								Name = _services.Translator.T($"vehicle.{key.ToKey()}", key),
								GameObject = gameObject,
								Thumbnail = ThumbnailGenerator.GetThumbnail(gameObject),
								Amt = data,
							});
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
				int category = _categories.Keys.ToList().IndexOf("Mod items");

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

							items.Add(new Item() {
								Name = _services.Translator.T($"item.{gameObject.name.ToKey()}", gameObject.name),
								Category = category,
								GameObject = gameObject,
								Thumbnail = ThumbnailGenerator.GetThumbnail(gameObject),
								Amt = data,
							});
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
			int category = _categories.Keys.ToList().IndexOf("Mod items");

			foreach (GameObject item in ModLoader.Database.GetAllItems())
			{
				try
				{
					items.Add(new Item() {
						Name = _services.Translator.T($"item.{item.name.ToKey()}", item.name),
						Category = category,
						GameObject = item,
						Thumbnail = ThumbnailGenerator.GetThumbnail(item),
					});
				}
				catch (Exception ex)
				{
					Logger.Log($"Failed to load mod item {(item?.name ?? "Unknown")} - {ex}", Logger.LogLevel.Error);
				}
			}

			return items;
		}

		/// <summary>
		/// Get the category for a given item.
		/// </summary>
		/// <param name="gameObject">The item to get the category for</param>
		/// <returns>The category index</returns>
		private int GetCategory(GameObject gameObject)
		{
			// Get all components, add types to list.
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			Dictionary<Type, MonoBehaviour> types = new Dictionary<Type, MonoBehaviour>();
			foreach (MonoBehaviour component in components)
			{
				if (!types.ContainsKey(component.GetType()))
					types.Add(component.GetType(), component);
			}

			// Convert keys to list to get the index later.
			List<string> names = _categories.Keys.ToList();

			int databaseLength = Enum.GetNames(typeof(itemdatabase.i)).Length;

			// Categories will be located in order.
			foreach (KeyValuePair<string, List<Type>> category in _categories)
			{
				foreach (Type type in category.Value)
				{
					// Use tosaveitemscript as a throwaway category for mod items as they
					// can't be found in the usual way.
					if (type == typeof(tosaveitemscript))
					{
						// Check if object index is outside the bounds of the regular itemdatabase.
						int index = Array.FindIndex(itemdatabase.d.items, i => i == gameObject);
						if (index >= databaseLength)
							return names.IndexOf("Mod items");
					}
					else if (types.ContainsKey(type))
					{
						MonoBehaviour component = types[type];
						if (type == typeof(pickupable))
						{
							pickupable pickupable = component as pickupable;
							if (pickupable.usable != null)
								return names.IndexOf(category.Key);
						}
						else
							return names.IndexOf(category.Key);
					}
				}
			}

			return names.IndexOf("Other");
		}
	}
}
