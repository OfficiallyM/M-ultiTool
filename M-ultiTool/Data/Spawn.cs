using MultiTool.Save;
using MultiTool.UI;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Data
{
	/// <summary>
	/// Spawn-related utilities.
	/// </summary>
	internal static class SpawnUtilities
	{
		/// <summary>
		/// Wrapper around the default spawn function to handle condition and fuel for items.
		/// </summary>
		/// <param name="item">The object to spawn</param>
		/// <param name="spawnConfig">Configuration for the spawned item</param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns>Spawned GameObject</returns>
		internal static GameObject Spawn(Item item, ItemSpawnConfig spawnConfig, Vector3? position = null, Quaternion? rotation = null)
		{
			try
			{
				GameObject spawned = null;

				int spawnCondition = spawnConfig.Condition;
				// Temporarily override the spawn condition when using random
				// to fully randomise it post-spawn.
				if (spawnCondition == -1)
					spawnCondition = 0;

				// Set license plate text on the prefab as GetComponentsInChildren()
				// doesn't find the plate of the spawned item.
				if (spawnConfig.Plate != string.Empty)
				{
					rendszamscript[] plateScripts = item.GameObject.GetComponentsInChildren<rendszamscript>();
					foreach (rendszamscript plateScript in plateScripts)
					{
						if (plateScript == null)
							continue;

						plateScript.Same(spawnConfig.Plate);
					}
				}

				bool amt = false;
				// AMT support.
				if (item.Amt != null)
				{
					amt = true;
					if (position == null)
						position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
					if (rotation == null)
						rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right);

					spawned = item.Amt.spawnMethod.Invoke(item.Amt.modItem, new object[] { position, rotation, spawnConfig.Condition, Colour.GetColour() }) as GameObject;
				}
				else
					spawned = Spawn(item.GameObject, Colour.GetColour(), spawnCondition, item.Variant ?? -1, position, rotation);

				// Return early if object spawning failed.
				if (spawned == null) return null;

				// Randomise item condition.
				if (spawnConfig.Condition == -1)
				{
					var partconditionscript = spawned.GetComponent<partconditionscript>();
					if (partconditionscript != null)
						GameUtilities.RandomiseCondition(partconditionscript);
				}

				// Reset prefab plate so it doesn't persist between spawns when unset.
				if (spawnConfig.Plate != string.Empty)
				{
					rendszamscript[] plateScripts = item.GameObject.GetComponentsInChildren<rendszamscript>();
					foreach (rendszamscript plateScript in plateScripts)
					{
						if (plateScript == null)
							continue;

						plateScript.same = false;
					}
				}

				tankscript fuelTank = spawned.GetComponent<tankscript>();
				bool amtTank = false;

				// AMT fluid support.
				if (amt)
				{
					Type propertiesType = item.Amt.modItem.GetType().Assembly.GetType("Amt.Vehicles.VehicleProperties");
					Component properties = spawned.GetComponent(propertiesType);
					mainscript.fluidcontainer container = properties.GetType().GetField("fuelContainer", BindingFlags.Instance | BindingFlags.Public).GetValue(properties) as mainscript.fluidcontainer;
					if (container != null)
					{
						fuelTank = new tankscript
						{
							F = container,
						};
						amtTank = true;
					}
				}
				else if (fuelTank == null)
					// Find fuel tank objects.
					fuelTank = spawned.GetComponentInChildren<tankscript>();

				if (fuelTank != null || amtTank)
				{

					// Fuel type and value are default, just spawn the item.
					bool alterFluids = false;
					if (spawnConfig.FuelMixCount >= 1 && (spawnConfig.FuelTypes[0] != -1 || spawnConfig.FuelValues[0] != -1f))
						alterFluids = true;

					// Support for spawning without any fuel.
					if (!spawnConfig.SpawnWithFuel)
					{
						fuelTank.F.fluids.Clear();
						alterFluids = false;
					}

					if (alterFluids)
					{
						// Store the current fuel types and amounts to return either to default.
						List<mainscript.fluidenum> currentFuelTypes = new List<mainscript.fluidenum>();
						List<float> currentFuelAmounts = new List<float>();
						foreach (mainscript.fluid fluid in fuelTank.F.fluids)
						{
							currentFuelTypes.Add(fluid.type);
							currentFuelAmounts.Add(fluid.amount);
						}

						fuelTank.F.fluids.Clear();

						for (int i = 0; i < spawnConfig.FuelMixCount; i++)
						{
							float amount = currentFuelAmounts.Count > i ? currentFuelAmounts[i] : 0;
							mainscript.fluidenum type = currentFuelTypes.Count > i ? currentFuelTypes[i] : mainscript.fluidenum.gas;

							if (spawnConfig.FuelValues[i] > -1)
								amount = spawnConfig.FuelValues[i];

							if (spawnConfig.FuelTypes[i] > -1)
								type = (mainscript.fluidenum)spawnConfig.FuelTypes[i];

							fuelTank.F.ChangeOne(amount, type);
						}
					}
				}				
			}
			catch (Exception ex)
			{
				Logger.Log($"Item spawning error - {ex}", Logger.LogLevel.Error);
			}

			return null;
		}

		/// <summary>
		/// Spawn a point of interest
		/// </summary>
		/// <param name="poi">The point of interest to spawn</param>
		/// <param name="spawnItems">Whether the POI should spawn items</param>
		/// <param name="position">Position override</param>
		/// <param name="rotation">Rotation override</param>
		/// <returns>The spawned point of interest</returns>
		internal static SpawnedPOI Spawn(Poi poi, bool spawnItems, Vector3? position = null, Quaternion? rotation = null)
		{
			GameObject gameObject = null;
			string id = string.Empty;
			try
			{
				bool save = true;
				Vector3 pos = new Vector3();
				Quaternion rot = new Quaternion();

				// Set default position and rotation.
				pos = mainscript.M.player.lookPoint + mainscript.M.player.transform.forward * 5f;
				pos.y = mainscript.M.player.gameObject.transform.position.y;

				// Starter house and pyramid needs a different offset.
				if (poi.Obj.name == "haz02")
					pos += Vector3.up * 0.18f;
				else if (poi.Obj.name == "Pyramid1")
					pos -= Vector3.up * 4f;
				else
					pos -= Vector3.up * 0.85f;

				rot = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right);

				if (position != null && rotation != null)
				{
					pos = position.GetValueOrDefault();
					rot = rotation.GetValueOrDefault();
					save = false;
				}

				gameObject = UnityEngine.Object.Instantiate(poi.Obj, pos, rot, mainscript.M.terrainGenerationSettings.roadBuildingGeneration.parent);
				gameObject.SetActive(true);

				// Some unused buildings need a custom rotation.
				string[] rotateBuildings = new string[]
				{
					"toalettarium",
					"ROADSIGNS1"
				};
				if (rotateBuildings.Contains(poi.Obj.name))
				{
					Transform transform = gameObject.transform;
					Vector3 angle = transform.localEulerAngles;
					angle.x = -90f;
					transform.localEulerAngles = angle;
				}

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
					id = SaveUtilities.InsertPOI(gameObject.name, pos, rot);
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Error spawning POI - {ex}", Logger.LogLevel.Error);
			}

			return new SpawnedPOI()
			{
				ID = id,
				PoiObject = gameObject,
				Data = poi,
			};
		}

		/// <summary>
		/// Based off mainscript Spawn method
		/// </summary>
		internal static GameObject Spawn(GameObject gameObject, Color color, int condition, int variant, Vector3? position = null, Quaternion? rotation = null)
		{
			if (position == null)
				position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
			if (rotation == null)
				rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.mainCam.transform.right);
			try
			{
				GameObject spawned = UnityEngine.Object.Instantiate(gameObject, position.Value, rotation.Value);

				partconditionscript conditionscript = spawned.GetComponent<partconditionscript>();
				if (conditionscript == null && spawned.GetComponent<childunparent>() != null)
					conditionscript = spawned.GetComponent<childunparent>().g.GetComponent<partconditionscript>();
				if (conditionscript != null)
				{
					if (variant != -1)
					{
						randomTypeSelector component2 = conditionscript.GetComponent<randomTypeSelector>();
						if (component2 != null)
						{
							component2.forceStart = false;
							component2.rtipus = variant;
							component2.Refresh();
						}
					}

					if (condition == -1)
						GameUtilities.RandomiseCondition(conditionscript);
					else
						GameUtilities.SetCondition(condition, false, conditionscript);
					GameUtilities.Paint(color, conditionscript, true);
				}

				mainscript.M.PostSpawn(spawned);

				return spawned;
			}
			catch (Exception ex)
			{
				Logger.Log($"Failed to spawn {gameObject.name} - {ex}", Logger.LogLevel.Error);
			}

			return null;
		}
	}
}
