using SpawnerTLD.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpawnerTLD.Modules
{
	internal class Utility
	{
		private Logger logger;

		public Utility(Logger _logger)
		{
			logger = _logger;
		}

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
				logger.Log($"Item spawning error - {ex}", Logger.LogLevel.Error);
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
		/// <returns>The spawned point of interest</returns>
		public GameObject Spawn(POI POI, bool spawnItems)
		{
			try
			{
				Vector3 position = mainscript.M.player.lookPoint;
				position.y = mainscript.M.player.gameObject.transform.position.y;

				// Don't apply offset to starter house.
				if (POI.poi.name == "haz02")
					position += Vector3.up * 0.2f;
				else
					position -= Vector3.up * 0.85f;

				GameObject gameObject = UnityEngine.Object.Instantiate(POI.poi, position, Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right), mainscript.M.terrainGenerationSettings.roadBuildingGeneration.parent);

				// TODO: This doesn't work.
				terrainHeightAlignToBuildingScript terrain = gameObject.GetComponent<terrainHeightAlignToBuildingScript>();
				if (terrain != null)
				{
					terrain.FStart(true);
				}

				if (spawnItems)
					gameObject.GetComponent<buildingscript>().FStart(0);

				return gameObject;
			}
			catch (Exception ex)
			{
				logger.Log($"Error spawning POI - {ex}", Logger.LogLevel.Error);
			}

			return null;
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
				logger.Log($"Failed to spawn {gameObject.name} - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Paint all child parts of a vehicle.
		/// </summary>
		/// <param name="c">The colour to paint</param>
		/// <param name="partconditionscript">The root vehicle partconditionscript</param>
		private void Paint(Color c, partconditionscript partconditionscript)
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
		private void FindPartChildren(partconditionscript root, ref List<partconditionscript> allChildren)
		{
			foreach (partconditionscript child in root.childs)
			{
				allChildren.Add(child);
				FindPartChildren(child, ref allChildren);
			}
		}
	}
}
