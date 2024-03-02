using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;
using Settings = MultiTool.Core.Settings;

namespace MultiTool.Utilities
{
    /// <summary>
    /// Spawn-related utilities.
    /// </summary>
    public static class SpawnUtilities
    {
		/// <summary>
		/// Wrapper around the default spawn function to handle condition and fuel for items.
		/// </summary>
		/// <param name="item">The object to spawn</param>
		internal static void Spawn(Item item)
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
				if (!new Settings().spawnWithFuel)
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
		internal static void Spawn(Vehicle vehicle)
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
			if (!new Settings().spawnWithFuel)
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
		internal static SpawnedPOI Spawn(POI POI, bool spawnItems, Vector3? position = null, Quaternion? rotation = null)
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

				if (position != null && rotation != null)
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
					ID = SaveUtilities.UpdatePOISaveData(new POIData()
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
		internal static void Spawn(GameObject gameObject, Color color, bool fullRandom, int condition, int variant)
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
						GameUtilities.RandomiseCondition(component1);
						GameUtilities.Paint(color, component1);
					}
					else
					{
						GameUtilities.SetConditionAndPaint(condition, color, component1);
					}
				}
				mainscript.M.PostSpawn(spawned);
			}
			catch (Exception ex)
			{
				Logger.Log($"Failed to spawn {gameObject.name} - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
