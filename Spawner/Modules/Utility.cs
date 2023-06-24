﻿using SpawnerTLD.Core;
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
		/// Wrapper around the default spawn function to handle condition and fuel for items.
		/// </summary>
		/// <param name="item">The object to spawn</param>
		public void Spawn(Item item)
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

		/// <summary>
		/// Wrapper around the default spawn function to extend vehicle functionality
		/// </summary>
		/// <param name="vehicle">The vehicle to spawn</param>
		public void Spawn(Vehicle vehicle)
		{
			int selectedCondition = vehicle.conditionInt;
			if (selectedCondition == -1)
			{
				// Randomise vehicle condition.
				int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
				vehicle.vehicle.GetComponent<partconditionscript>().StartFullRandom(0, maxCondition);
				selectedCondition = UnityEngine.Random.Range(0, maxCondition);
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
				mainscript.M.Spawn(vehicle.vehicle, vehicle.color, selectedCondition, vehicle.variant);
				return;
			}

			// Fuel type and value are default, just spawn the vehicle.
			if (vehicle.fuelMixes == 1)
			{
				if (vehicle.fuelTypeInts[0] == -1 && vehicle.fuelValues[0] == -1f)
				{
					mainscript.M.Spawn(vehicle.vehicle, vehicle.color, selectedCondition, vehicle.variant);
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
			mainscript.M.Spawn(vehicle.vehicle, vehicle.color, selectedCondition, vehicle.variant);
		}
	}
}
