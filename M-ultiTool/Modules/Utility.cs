using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TLDLoader;
using UnityEngine;
using Settings = MultiTool.Core.Settings;

namespace MultiTool.Modules
{
	internal class Utility
	{
		private Settings settings = new Settings();

		/// <summary>
		/// Check if an object is a vehicle.
		/// </summary>
		/// <param name="gameObject">The object to check</param>
		/// <returns>true if the object is a vehicle or trailer; otherwise, false</returns>
		public bool IsVehicleOrTrailer(GameObject gameObject)
		{
			if (gameObject.name.ToLower().Contains("full") && (gameObject.GetComponentsInChildren<carscript>().Length > 0 || gameObject.GetComponentsInChildren<utanfutoscript>().Length > 0))
				return true;
			return false;
		}

		/// <summary>
		/// Get the category for a given item.
		/// </summary>
		/// <param name="gameObject">The item to get the category for</param>
		/// <param name="categories">The categories to sort into</param>
		/// <returns>The category index</returns>
		public int GetCategory(GameObject gameObject, Dictionary<string, List<Type>> categories)
		{
			// Get all components, add types to list.
			var components = gameObject.GetComponents<MonoBehaviour>();
			Dictionary<Type, MonoBehaviour> types = new Dictionary<Type, MonoBehaviour>();
			foreach (var component in components)
			{
				if (!types.ContainsKey(component.GetType()))
					types.Add(component.GetType(), component);
			}

			// Convert keys to list to get the index later.
			List<string> names = categories.Keys.ToList();

			// Categories will be located in order.
			foreach (KeyValuePair<string, List<Type>> category in categories)
			{
				foreach (Type type in category.Value)
				{
					if (types.ContainsKey(type))
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

		/// <summary>
		/// Wrapper around the default spawn function to handle condition and fuel for items.
		/// </summary>
		/// <param name="item">The object to spawn</param>
		public void Spawn(Item item)
		{
			try
			{
				int selectedCondition = item.conditionInt;
				if (selectedCondition == -1)
				{
					// Randomise item condition.
					int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
					item.item.GetComponent<partconditionscript>().StartFullRandom(0, maxCondition);
					selectedCondition = UnityEngine.Random.Range(0, maxCondition);
				}

				tankscript fuelTank = item.item.GetComponent<tankscript>();

				// Find fuel tank objects.
				if (fuelTank == null)
				{
					fuelTank = item.item.GetComponentInChildren<tankscript>();
				}

				if (fuelTank == null)
				{
					// Item doesn't have a fuel tank, just spawn the item and return.
					mainscript.M.Spawn(item.item, item.color, selectedCondition, -1);
					return;
				}

				// Support for spawning without any fuel.
				if (!settings.spawnWithFuel)
				{
					fuelTank.F.fluids.Clear();
					mainscript.M.Spawn(item.item, item.color, selectedCondition, -1);
					return;
				}

				// Fuel type and value are default, just spawn the item.
				if (item.fuelMixes == 1)
				{
					if (item.fuelTypeInts[0] == -1 && item.fuelValues[0] == -1f)
					{
						mainscript.M.Spawn(item.item, item.color, selectedCondition, -1);
						return;
					}
				}

				// Store the current fuel types and amounts to return either to default.
				List<mainscript.fluidenum> currentFuelTypes = new List<mainscript.fluidenum>();
				List<float> currentFuelAmounts = new List<float>();
				foreach (mainscript.fluid fluid in fuelTank.F.fluids)
				{
					currentFuelTypes.Add(fluid.type);
					currentFuelAmounts.Add(fluid.amount);
				}

				fuelTank.F.fluids.Clear();

				for (int i = 0; i < item.fuelMixes; i++)
				{
					if (item.fuelTypeInts[i] == -1 && item.fuelValues[i] > -1)
					{
						fuelTank.F.ChangeOne(item.fuelValues[i], currentFuelTypes[i]);
					}
					else if (item.fuelTypeInts[i] > -1 && item.fuelValues[i] == -1)
					{
						fuelTank.F.ChangeOne(currentFuelAmounts[i], (mainscript.fluidenum)item.fuelTypeInts[i]);
					}
					else
					{
						fuelTank.F.ChangeOne(item.fuelValues[i], (mainscript.fluidenum)item.fuelTypeInts[i]);
					}
				}
				mainscript.M.Spawn(item.item, item.color, selectedCondition, -1);
			}
			catch (Exception ex)
			{
				Logger.Log($"Item spawning error - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Wrapper around the default spawn function to extend vehicle functionality
		/// </summary>
		/// <param name="vehicle">The vehicle to spawn</param>
		public void Spawn(Vehicle vehicle)
		{
			int selectedCondition = vehicle.conditionInt;
			bool fullRandom = false;
			if (selectedCondition == -1)
			{
				// Randomise vehicle condition.
				int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
				selectedCondition = UnityEngine.Random.Range(0, maxCondition);
				fullRandom = true;
			}

			// Set vehicle license plate text.
			if (vehicle.plate != String.Empty)
			{
				rendszamscript[] plateScripts = vehicle.vehicle.GetComponentsInChildren<rendszamscript>();
				foreach (rendszamscript plateScript in plateScripts)
				{
					if (plateScript == null)
						continue;

					plateScript.Same(vehicle.plate);
				}
			}

			tankscript fuelTank = vehicle.vehicle.GetComponent<tankscript>();

			// Find fuel tank objects.
			if (fuelTank == null)
			{
				fuelTank = vehicle.vehicle.GetComponentInChildren<tankscript>();
			}

			if (fuelTank == null)
			{
				// Vehicle doesn't have a fuel tank, just spawn the vehicle and return.
				Spawn(vehicle.vehicle, vehicle.color, fullRandom, selectedCondition, vehicle.variant);
				return;
			}

			// Support for spawning without any fuel.
			if (!settings.spawnWithFuel)
			{
				fuelTank.F.fluids.Clear();
				Spawn(vehicle.vehicle, vehicle.color, fullRandom, selectedCondition, vehicle.variant);
				return;
			}

			// Fuel type and value are default, just spawn the vehicle.
			if (vehicle.fuelMixes == 1)
			{
				if (vehicle.fuelTypeInts[0] == -1 && vehicle.fuelValues[0] == -1f)
				{
					Spawn(vehicle.vehicle, vehicle.color, fullRandom, selectedCondition, vehicle.variant);
					return;
				}
			}

			// Store the current fuel types and amounts to return either to default.
			List<mainscript.fluidenum> currentFuelTypes = new List<mainscript.fluidenum>();
			List<float> currentFuelAmounts = new List<float>();
			foreach (mainscript.fluid fluid in fuelTank.F.fluids)
			{
				currentFuelTypes.Add(fluid.type);
				currentFuelAmounts.Add(fluid.amount);
			}

			fuelTank.F.fluids.Clear();

			for (int i = 0; i < vehicle.fuelMixes; i++)
			{
				if (vehicle.fuelTypeInts[i] == -1 && vehicle.fuelValues[i] > -1)
				{
					fuelTank.F.ChangeOne(vehicle.fuelValues[i], currentFuelTypes[i]);
				}
				else if (vehicle.fuelTypeInts[i] > -1 && vehicle.fuelValues[i] == -1)
				{
					fuelTank.F.ChangeOne(currentFuelAmounts[i], (mainscript.fluidenum)vehicle.fuelTypeInts[i]);
				}
				else
				{
					fuelTank.F.ChangeOne(vehicle.fuelValues[i], (mainscript.fluidenum)vehicle.fuelTypeInts[i]);
				}
			}
			Spawn(vehicle.vehicle, vehicle.color, fullRandom, selectedCondition, vehicle.variant);
		}

		/// <summary>
		/// Spawn a point of interest
		/// </summary>
		/// <param name="POI">The point of interest to spawn</param>
		/// <param name="spawnItems">Whether the POI should spawn items</param>
		/// <param name="position">Position override</param>
		/// <param name="rotation">Rotation override</param>
		/// <returns>The spawned point of interest</returns>
		public SpawnedPOI Spawn(POI POI, bool spawnItems, Vector3? position = null, Quaternion? rotation = null)
		{
			GameObject gameObject = null;
			int ID = -1;
			try
			{
				bool save = true;
				Vector3 pos = new Vector3();
				Quaternion rot = new Quaternion();

				// Set default position and rotation.
				pos = mainscript.M.player.lookPoint + mainscript.M.player.transform.forward * 5f;
				pos.y = mainscript.M.player.gameObject.transform.position.y;

				// Starter house needs a different offset.
				if (POI.poi.name == "haz02")
					pos += Vector3.up * 0.18f;
				else
					pos -= Vector3.up * 0.85f;

				rot = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right);

				if (position != null && position != null)
				{
					pos = position.GetValueOrDefault();
					rot = rotation.GetValueOrDefault();
					save = false;
				}

				//var components = POI.poi.GetComponents<MonoBehaviour>();
				//foreach (var component in components)
				//	Logger.Log($"{component.GetType()}", Logger.LogLevel.Debug);

				gameObject = UnityEngine.Object.Instantiate(POI.poi, pos, rot, mainscript.M.terrainGenerationSettings.roadBuildingGeneration.parent);

				// TODO: Does fuck all.
				// Find appropriate terrainHeightAlignToBuildingScript from TerrainGenerator.
				//terrainHeightAlignToBuildingScript terrain = TerrainGenerator.TG.buildings.Where(b => b.name.Contains(POI.poi.name)).FirstOrDefault();
				//if (terrain != null)
				//{
				//	terrain.FStart(true);
				//}

				// TODO: Also does fuck all.
				//foreach (digholescript2 componentsInChild in gameObject.GetComponentsInChildren<digholescript2>())
				//{
				//	componentsInChild.Refresh();
				//}

				buildingscript buildingscript = gameObject.GetComponent<buildingscript>();
				if (buildingscript != null)
				{
					buildingscript.itemsSpawned = !spawnItems;

					// Force start building script.
					buildingscript.FStart(0);
				}

				// Save the POI.
				if (save)
				{
					ID = UpdatePOISaveData(new POIData()
					{
						poi = gameObject.name,
						position = pos,
						rotation = rot,
					});
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Error spawning POI - {ex}", Logger.LogLevel.Error);
			}

			return new SpawnedPOI()
			{
				ID = ID,
				poi = gameObject,
			};
		}

		/// <summary>
		/// Based off mainscript Spawn method
		/// </summary>
		public void Spawn(GameObject gameObject, Color color, bool fullRandom, int condition, int variant)
		{
			try
			{
				GameObject spawned = UnityEngine.Object.Instantiate(gameObject, mainscript.M.player.lookPoint + Vector3.up * 0.75f, Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right));
				partconditionscript component1 = spawned.GetComponent<partconditionscript>();
				if (component1 == null && spawned.GetComponent<childunparent>() != null)
					component1 = spawned.GetComponent<childunparent>().g.GetComponent<partconditionscript>();
				if (component1 != null)
				{
					if (variant != -1)
					{
						randomTypeSelector component2 = component1.GetComponent<randomTypeSelector>();
						if (component2 != null)
						{
							component2.forceStart = false;
							component2.rtipus = variant;
							component2.Refresh();
						}
					}

					if (fullRandom)
					{
						RandomiseCondition(component1);
					}
					else
					{
						component1.StartPaint(condition, color);
					}

					Paint(color, component1);
				}
				mainscript.M.PostSpawn(spawned);
			}
			catch (Exception ex)
			{
				Logger.Log($"Failed to spawn {gameObject.name} - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Read/write data to game save
		/// <para>Originally from RundensWheelPositionEditor</para>
		/// </summary>
		/// <param name="input">The string to write to the save</param>
		/// <returns>The read/written string</returns>
		public string ReadWriteToGameSave(string input = null)
		{
			try
			{
				save_rendszam saveRendszam = null;
				save_prefab savePrefab1;

				// Attempt to find existing plate.
				if ((savedatascript.d.data.farStuff.TryGetValue(Mathf.Abs(Meta.ID.GetHashCode()), out savePrefab1) || savedatascript.d.data.nearStuff.TryGetValue(Mathf.Abs(Meta.ID.GetHashCode()), out savePrefab1)) && savePrefab1.rendszam != null)
					saveRendszam = savePrefab1.rendszam;

				// Plate doesn't exist.
				if (saveRendszam == null)
				{
					// Create a new plate to store the input string in.
					tosaveitemscript component = itemdatabase.d.gplate.GetComponent<tosaveitemscript>();
					save_prefab savePrefab2 = new save_prefab(component.category, component.id, double.MaxValue, double.MaxValue, double.MaxValue, 0.0f, 0.0f, 0.0f);
					savePrefab2.rendszam = new save_rendszam();
					saveRendszam = savePrefab2.rendszam;
					saveRendszam.S = string.Empty;
					savedatascript.d.data.farStuff.Add(Mathf.Abs(Meta.ID.GetHashCode()), savePrefab2);
				}

				// Write the input to the plate.
				if (input != null && input != string.Empty)
					saveRendszam.S = input;

				return saveRendszam.S;
			}
			catch (Exception ex)
			{
				Logger.Log($"Save read/write error - {ex}", Logger.LogLevel.Error);
			}

			return string.Empty;
		}

		/// <summary>
		/// Unserialize existing save data
		/// </summary>
		/// <returns>Unserialized save data</returns>
		public Save UnserializeSaveData()
		{
			string existingString = ReadWriteToGameSave();
			if (existingString == null || existingString == string.Empty)
				return new Save();

			MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(existingString));
			DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Save));
			return jsonSerializer.ReadObject(ms) as Save;
		}

		/// <summary>
		/// Serialize save data and write to save
		/// </summary>
		/// <param name="data">The data to serialize</param>
		public void SerializeSaveData(Save data)
		{
			MemoryStream ms = new MemoryStream();
			DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Save));
			jsonSerializer.WriteObject(ms, data);

			// Rewind stream.
			ms.Seek(0, SeekOrigin.Begin);

			// Convert stream to a string.
			StreamReader reader = new StreamReader(ms);
			string jsonString = reader.ReadToEnd();

			ReadWriteToGameSave(jsonString);
		}

		/// <summary>
		/// Update POI data in save
		/// </summary>
		/// <param name="poi">The POI to update</param>
		/// <param name="type">Update type, either "insert" or "delete"</param>
		/// <returns>POI ID</returns>
		public int UpdatePOISaveData(POIData poi, string type = "insert")
		{
			Save data = UnserializeSaveData();

			int ID = -1;

			try
			{
				switch (type)
				{
					case "insert":
						if (data.pois == null)
							data.pois = new List<POIData>();

						poi.ID = data.pois.Count;
						ID = poi.ID;

						data.pois.Add(poi);
						break;
					case "delete":
						POIData poiData = data.pois.First(p => p.ID == poi.ID);
						if (poiData != null)
						{
							ID = poiData.ID;
							data.pois.Remove(poiData);
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"POI update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);

			return ID;
		}

		/// <summary>
		/// Update glass data in save
		/// </summary>
		/// <param name="glas">Glass data</param>
		public void UpdateGlass(GlassData glass)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.glass == null)
					data.glass = new List<GlassData>();

				GlassData existing = data.glass.Where(g => g.ID == glass.ID && g.type == glass.type).FirstOrDefault();
				if (existing != null)
					existing.color = glass.color;
				else
					data.glass.Add(glass);
			}
			catch (Exception ex)
			{
				Logger.Log($"Glass update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Paint all child parts of a vehicle.
		/// </summary>
		/// <param name="c">The colour to paint</param>
		/// <param name="partconditionscript">The root vehicle partconditionscript</param>
		public void Paint(Color c, partconditionscript partconditionscript)
		{
			partconditionscript.Paint(c);
			foreach (partconditionscript child in partconditionscript.childs)
			{
				if (!child.isChild && ! child.loaded)
					Paint(c, child);
			}
		}

		/// <summary>
		/// Randomise condition of all parts
		/// </summary>
		/// <param name="partconditionscript">Base vehicle partconditionscript</param>
		private void RandomiseCondition(partconditionscript partconditionscript)
		{
			List<partconditionscript> children = new List<partconditionscript>();
			FindPartChildren(partconditionscript, ref children);

			foreach (partconditionscript child in children)
			{
				child.RandomState(0, 4);
				child.Refresh();
			}
		}

		/// <summary>
		/// Recursively find all child parts
		/// </summary>
		/// <param name="root">Parent part</param>
		/// <param name="allChildren">Current list of child parts</param>
		public void FindPartChildren(partconditionscript root, ref List<partconditionscript> allChildren)
		{
			foreach (partconditionscript child in root.childs)
			{
				allChildren.Add(child);
				FindPartChildren(child, ref allChildren);
			}
		}

		/// <summary>
		/// Migrate SpawnerTLD config to M-ultiTool.
		/// </summary>
		public void MigrateFromSpawner()
		{
			string spawnerSettings = Path.Combine(ModLoader.ModsFolder, "Config", "Mod Settings", "SpawnerTLD");
			string path = ModLoader.GetModConfigFolder(MultiTool.mod);
			if (File.Exists(Path.Combine(spawnerSettings, "Config.json")))
			{
				// Delete existing config if it exists.
				if (File.Exists(Path.Combine(path, "Config.json")))
				{
					try
					{
						File.Delete(Path.Combine(path, "Config.json"));
					}
					catch (Exception ex)
					{
						Logger.Log($"Error removing M-ultiTool Config.json - {ex}", Logger.LogLevel.Error);
						return;
					}
				}

				try
				{
					File.Move(Path.Combine(spawnerSettings, "Config.json"), Path.Combine(path, "Config.json"));
					Logger.Log("Successfully migrated config from SpawnerTLD to M-ultiTool.");
				}
				catch (Exception ex)
				{
					Logger.Log($"Error migrating config from SpawnerTLD to M-ultiTool - {ex}", Logger.LogLevel.Error);
				}
			}
		}
	}
}
