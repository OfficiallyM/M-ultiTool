using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		internal static GameObject Spawn(Item item, Vector3? position = null, Quaternion? rotation = null)
        {
            try
            {
                bool amt = false;
                // AMT support.
                if (item.amt != null)
                {
                    amt = true;
                    if (position == null)
                        position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
                    if (rotation == null)
                        rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right);

                    item.gameObject = item.amt.spawnMethod.Invoke(item.amt.modItem, new object[] { position, rotation, item.conditionInt, item.color }) as GameObject;
                }

                int selectedCondition = item.conditionInt;
                if (selectedCondition == -1 && item.gameObject.GetComponent<partconditionscript>() != null)
                {
                    // Randomise item condition.
                    int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
                    item.gameObject.GetComponent<partconditionscript>().StartFullRandom(0, maxCondition);
                    selectedCondition = UnityEngine.Random.Range(0, maxCondition);
                }

                tankscript fuelTank = item.gameObject.GetComponent<tankscript>();
                bool amtTank = false;

                // AMT fluid support.
                if (amt)
                {
                    Type propertiesType = item.amt.modItem.GetType().Assembly.GetType("Amt.Vehicles.VehicleProperties");
                    var properties = item.gameObject.GetComponent(propertiesType);
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
                    fuelTank = item.gameObject.GetComponentInChildren<tankscript>();

                if (fuelTank != null || amtTank)
                {
                    // Support for spawning without any fuel.
                    if (!new Settings().spawnWithFuel)
                    {
                        fuelTank.F.fluids.Clear();
                    }

                    // Fuel type and value are default, just spawn the item.
                    bool alterFluids = false;
                    if (item.fuelMixes >= 1 && item.fuelTypeInts[0] != -1 && item.fuelValues[0] != -1f)
                        alterFluids = true;

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

				        for (int i = 0; i < item.fuelMixes; i++)
				        {
                            float amount = currentFuelAmounts[i];
                            mainscript.fluidenum type = currentFuelTypes[i];

                            if (item.fuelValues[i] > -1)
                                amount = item.fuelValues[i];

                            if (item.fuelTypeInts[i] > -1)
                                type = (mainscript.fluidenum)item.fuelTypeInts[i];

						    fuelTank.F.ChangeOne(amount, type);
				        }
                    }
                }

                if (amt)
                    return item.gameObject;
                else
                    return Spawn(item.gameObject, item.color, selectedCondition, -1, position, rotation);
			}
			catch (Exception ex)
			{
				Logger.Log($"Item spawning error - {ex}", Logger.LogLevel.Error);
			}

			return null;
		}

		/// <summary>
		/// Wrapper around the default spawn function to extend vehicle functionality
		/// </summary>
		/// <param name="vehicle">The vehicle to spawn</param>
		internal static void Spawn(Vehicle vehicle)
		{
            int selectedCondition = vehicle.conditionInt;
			if (selectedCondition == -1)
			{
				// Randomise vehicle condition.
				int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
				selectedCondition = UnityEngine.Random.Range(0, maxCondition);
			}

			// Set vehicle license plate text on the prefab as GetComponentsInChildren()
			// doesn't find the plate of the spawned vehicle.
			if (vehicle.plate != String.Empty)
			{
				rendszamscript[] plateScripts = vehicle.gameObject.GetComponentsInChildren<rendszamscript>();
				foreach (rendszamscript plateScript in plateScripts)
				{
					if (plateScript == null)
						continue;

					plateScript.Same(vehicle.plate);
				}
			}

            GameObject spawnedVehicle = null;
            bool amt = false;
            // AMT support.
            if (vehicle.amt != null)
            {
                amt = true;
                Vector3 position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right);

                spawnedVehicle = vehicle.amt.spawnMethod.Invoke(vehicle.amt.modItem, new object[] { position, rotation, vehicle.conditionInt, vehicle.color }) as GameObject;
            }
            else
			    spawnedVehicle = Spawn(vehicle.gameObject, vehicle.color, selectedCondition, vehicle.variant);

			// Error occurred during vehicle spawn, return early.
			if (spawnedVehicle == null) return;

			// Reset prefab plate so it doesn't persist between spawns when unset.
			if (vehicle.plate != String.Empty)
			{
				rendszamscript[] plateScripts = vehicle.gameObject.GetComponentsInChildren<rendszamscript>();
				foreach (rendszamscript plateScript in plateScripts)
				{
					if (plateScript == null)
						continue;

					plateScript.same = false;
				}
			}

            tankscript fuelTank = spawnedVehicle.GetComponent<tankscript>();
            bool amtTank = false;
            // AMT fluid support.
            if (amt)
            {
                Type propertiesType = vehicle.amt.modItem.GetType().Assembly.GetType("Amt.Vehicles.VehicleProperties");
                var properties = spawnedVehicle.GetComponent(propertiesType);
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
                fuelTank = spawnedVehicle.GetComponentInChildren<tankscript>();

            if (fuelTank != null || amtTank)
            {
                // Support for spawning without any fuel.
                if (!new Settings().spawnWithFuel)
                {
                    fuelTank.F.fluids.Clear();
                }

                // Fuel type and value are default, just spawn the item.
                bool alterFluids = false;
                if (vehicle.fuelMixes >= 1 && vehicle.fuelTypeInts[0] != -1 && vehicle.fuelValues[0] != -1f)
                    alterFluids = true;

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

                    for (int i = 0; i < vehicle.fuelMixes; i++)
                    {
                        float amount = currentFuelAmounts.Count > i ? currentFuelAmounts[i] : 0;
                        mainscript.fluidenum type = currentFuelTypes.Count > i ? currentFuelTypes[i] : mainscript.fluidenum.gas;

                        if (vehicle.fuelValues[i] > -1)
                            amount = vehicle.fuelValues[i];

                        if (vehicle.fuelTypeInts[i] > -1)
                            type = (mainscript.fluidenum)vehicle.fuelTypeInts[i];

                        fuelTank.F.ChangeOne(amount, type);
                    }
                }
            }
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
		internal static GameObject Spawn(GameObject gameObject, Color color, int condition, int variant, Vector3? position = null, Quaternion? rotation = null)
		{
			if (position == null)
				position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
			if (rotation == null)
				rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right);
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
