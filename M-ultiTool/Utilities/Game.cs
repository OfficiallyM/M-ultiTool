﻿using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Utilities
{
	/// <summary>
	/// Game interaction utilities.
	/// </summary>
	public static class GameUtilities
	{
		/// <summary>
		/// Check if an object is a vehicle.
		/// </summary>
		/// <param name="gameObject">The object to check</param>
		/// <returns>true if the object is a vehicle or trailer; otherwise, false</returns>
		public static bool IsVehicleOrTrailer(GameObject gameObject, bool strict = true)
		{
			string name = gameObject.name.ToLower();
			bool nameCheck = name.Contains("full");

			// Non-strict name check includes all items containing car or bus.
			if (!strict)
				nameCheck = name.Contains("full") || name.Contains("car") || name.Contains("bus");

			if (nameCheck && (gameObject.GetComponentsInChildren<carscript>().Length > 0 || gameObject.GetComponentsInChildren<utanfutoscript>().Length > 0))
				return true;
			return false;
		}

		/// <summary>
		/// Get the category for a given item.
		/// </summary>
		/// <param name="gameObject">The item to get the category for</param>
		/// <param name="categories">The categories to sort into</param>
		/// <returns>The category index</returns>
		public static int GetCategory(GameObject gameObject)
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
			List<string> names = GUIRenderer.categories.Keys.ToList();

			int databaseLength = Enum.GetNames(typeof(itemdatabase.i)).Length;

			// Categories will be located in order.
			foreach (KeyValuePair<string, List<Type>> category in GUIRenderer.categories)
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

		/// <summary>
		/// Paint all child parts of a vehicle.
		/// </summary>
		/// <param name="c">The colour to paint</param>
		/// <param name="root">The root vehicle partconditionscript</param>
		public static void Paint(Color c, partconditionscript root, bool useSlotChildren = false)
		{
			List<partconditionscript> parts = FindPartChildren(root);
            if (useSlotChildren)
                FindPartChildren(root, ref parts);

			foreach (partconditionscript part in parts)
			{
				if (!part.IsPaintable()) continue;

				part.Refresh(part.state, c);
			}
		}

		/// <summary>
		/// Set vehicle condition.
		/// </summary>
		/// <param name="condition">Condition state to set</param>
		/// <param name="applyToAttached">True to apply condition to any attached objects, false to apply to just vehicle</param>
		/// <param name="root">Root vehicle partconditionscript</param>
		public static void SetCondition(int condition, bool applyToAttached, partconditionscript root)
		{
			List<partconditionscript> children = new List<partconditionscript>();
			if (applyToAttached)
				children = FindPartChildren(root);
			else
				FindPartChildren(root, ref children);

			foreach (partconditionscript child in children)
			{
				child.Refresh(condition);
			}
		}

		/// <summary>
		/// Set condition and paint of root vehicle and all child parts
		/// </summary>
		/// <param name="condition">Condition state to set</param>
		/// <param name="color">Color to set</param>
		/// <param name="root">Root vehicle partconditionscript</param>
		public static void SetConditionAndPaint(int condition, Color color, partconditionscript root)
		{
			List<partconditionscript> parts = FindPartChildren(root);
			foreach (partconditionscript part in parts)
			{
				if (!part.IsPaintable())
					part.Refresh(condition);
				else
					part.Refresh(condition, color);
			}
		}

		/// <summary>
		/// Randomise condition of all parts.
		/// </summary>
		/// <param name="partconditionscript">Base vehicle partconditionscript</param>
		public static void RandomiseCondition(partconditionscript partconditionscript)
		{
			List<partconditionscript> children = FindPartChildren(partconditionscript);

			foreach (partconditionscript child in children)
			{
				child.RandomState(0, 4);
				child.Refresh();
			}
		}

		/// <summary>
		/// Get all child parts.
		/// </summary>
		/// <param name="root">Root part</param>
		/// <returns>List of all child parts</returns>
		public static List<partconditionscript> FindPartChildren(partconditionscript root, bool requireToSave = true)
		{
			List<partconditionscript> parts = root.GetComponentsInChildren<partconditionscript>().ToList();
			List<partconditionscript> returnParts = new List<partconditionscript>();
			foreach (partconditionscript part in parts)
			{
				if (part.DontUseWorn)
					continue;

				if (requireToSave && part.tosave == null)
					continue;

				returnParts.Add(part);
			}

			// Couldn't find any parts, attempt to find using part children.
			if (returnParts.Count == 0)
				FindPartChildren(root, ref returnParts);

			return returnParts;
		}

		/// <summary>
		/// Find all child parts recursively with tosaveitemscript.
		/// </summary>
		/// <param name="root">Root part</param>
		/// <param name="children">Child partconditionscript passed by reference</param>
		public static void FindPartChildren(partconditionscript root, ref List<partconditionscript> children)
		{
			if (!children.Contains(root)) children.Add(root);

			tosaveitemscript tosave = root.GetComponent<tosaveitemscript>();
			if (tosave == null) return;

			foreach (partslotscript slot in tosave.partslotscripts)
			{
				if (slot.part == null || slot.part.condition == null)
					continue;

				children.Add(slot.part.condition);
				FindPartChildren(slot.part.condition, ref children);
			}
		}

		/// <summary>
		/// Check if part is paintable.
		/// </summary>
		/// <param name="part">Part to check</param>
		/// <returns>True if part is paintable, otherwise false</returns>
		public static bool IsPaintable(this partconditionscript part)
		{
			return part.forceColor || !part.disableColor;
		}

		/// <summary>
		/// Get all parts for a vehicle.
		/// </summary>
		/// <param name="vehicle">Vehicle to get parts for</param>
		/// <returns>Current list of child parts</returns>
		public static List<partconditionscript> GetVehicleParts(GameObject vehicle, bool requireToSave = true)
		{
			List<partconditionscript> parts = new List<partconditionscript>();
			partconditionscript vehicleCondition = vehicle.GetComponent<partconditionscript>();
			if (vehicleCondition == null)
				return parts;

			return FindPartChildren(vehicleCondition, requireToSave);
		}

		/// <summary>
		/// Get specific vehicle part by name.
		/// </summary>
		/// <param name="vehicle">Vehicle to get part from</param>
		/// <param name="name">Name of part to find</param>
		/// <returns>Part if name exists, otherwise null</returns>
		public static partconditionscript GetVehiclePartByName(GameObject vehicle, string name, bool requireToSave = true)
		{
			List<partconditionscript> parts = GetVehicleParts(vehicle, requireToSave);
			return parts.Where(part => part.name.ToLower() == name.ToLower()).FirstOrDefault();
		}

		/// <summary>
		/// Get all parts matching a partial name.
		/// </summary>
		/// <param name="vehicle">Vehicle to get part from</param>
		/// <param name="name">Partial name of parts to find</param>
		/// <returns>List of parts if name matches any, otherwise empty list</returns>
		public static List<partconditionscript> GetVehiclePartsByPartialName(GameObject vehicle, string name, bool requireToSave = true)
		{
			List<partconditionscript> parts = GetVehicleParts(vehicle, requireToSave);
			return parts.Where(part => part.name.ToLower().Contains(name.ToLower())).ToList();
		}

        /// <summary>
        /// Find specific conditionless part by name.
        /// </summary>
        /// <param name="vehicle">Vehicle to get part from</param>
        /// <param name="name">Name of part to find</param>
        /// <returns>Part mesh if exists, otherwise null</returns>
        public static MeshRenderer GetConditionlessVehiclePartByName(GameObject vehicle, string name)
        {
            MeshRenderer[] meshes = vehicle.GetComponentsInChildren<MeshRenderer>();
            return meshes.Where(m => m.name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        /// <summary>
        /// Find conditionless parts by name.
        /// </summary>
        /// <param name="vehicle">Vehicle to get part from</param>
        /// <param name="name">Name of part to find</param>
        /// <returns>List of meshes if name matches anty, otherwise empty list</returns>
        public static List<MeshRenderer> GetConditionlessVehiclePartsByName(GameObject vehicle, string name)
        {
            MeshRenderer[] meshes = vehicle.GetComponentsInChildren<MeshRenderer>();
            return meshes.Where(m => m.name.ToLower().Contains(name.ToLower())).ToList();
        }

        /// <summary>
        /// Set material of a part.
        /// </summary>
        /// <param name="part">Part to set material for</param>
        /// <param name="type">Material type to set</param>
        /// <param name="color">Optional material colour</param>
        public static void SetPartMaterial(partconditionscript part, string type, Color? color = null)
		{
			// Remove all existing materials.
			part.mNew = null;
			part.mUsed = null;
			part.mMiddle = null;
			part.mOld = null;
			part.mRusty = null;

			// Set the new type.
			if (part.useOnlyMaterialTipusForMaterial)
				part.materialTipus = type;
			else
				part.tipus = type;

			// Force part colour.
			if (color != null)
			{
				Color c = color.Value;
				part.color = c;
				part.forceColor = true;
				part.disableColor = false;
			}
			else
			{
				part.disableColor = true;
			}

			// Re-start the part to force the material change.
			part.started = false;
			part.randomStart = false;
			part.useRandomTipus = false;
			part.loaded = true;
			part.FStart();
		}

        /// <summary>
        /// Set the material of a part without condition support.
        /// </summary>
        /// <param name="mesh">Mesh to set material on</param>
        /// <param name="type">Material type</param>
        /// <param name="color">Optional material colour</param>
        public static void SetConditionlessPartMaterial(MeshRenderer mesh, string type, Color? color = null)
        {
            mainscript.conditionmaterial material = mainscript.M.conditionmaterials.Where(m => m.tipus == type).FirstOrDefault();
            if (material == null) return;
            // Default to new condition material for now.
            // TODO: Make this user customisable?
            mesh.material = material.New;
            if (color != null)
            {
                mesh.material.color = color.Value;
            }
        }

        /// <summary>
        /// Set headlight color.
        /// </summary>
        /// <param name="headlight">Headlight to set color of</param>
        /// <param name="color">Color to set</param>
        /// <param name="isInteriorLight">True if light is an interior light, false if it's a standard headlight</param>
        public static void SetHeadlightColor(headlightscript headlight, Color color, bool isInteriorLight = false)
        {
            Color offColor = color.ChangeBrightness(-0.1f);
            Color brightColor = color.ChangeBrightness(0.1f);

            headlight.MOFF = new Material(headlight.MOFF)
            {
                color = offColor
            };
            for (int stateIndex = 0; stateIndex < headlight.states.Count; stateIndex++)
            {
                headlightscript.lightState state = headlight.states[stateIndex];
                if (isInteriorLight)
                {
                    SetHeadlightStateColor(state, color);
                }
                else
                {
                    switch (stateIndex)
                    {
                        case 0:
                            SetHeadlightStateColor(state, offColor);
                            break;
                        case 2:
                            SetHeadlightStateColor(state, brightColor);
                            break;
                        default:
                            SetHeadlightStateColor(state, color);
                            break;
                    }
                }
            }
            headlight.RefreshLights();
        }

        /// <summary>
        /// Set headlight state color.
        /// </summary>
        /// <param name="state">Headlight state to update</param>
        /// <param name="color">Color to set</param>
        public static void SetHeadlightStateColor(headlightscript.lightState state, Color color)
        {
            if (state.L != null)
                state.L.color = color;

            if (state.M != null)
            {
                state.M = new Material(state.M)
                {
                    color = color
                };
                state.M.SetColor("_EmissionColor", color);
            }

            foreach (Light l in state.Ls)
            {
                l.color = color;
            }
        }

		/// <summary>
		/// Get global position of an object.
		/// </summary>
		/// <param name="objPos">Object to get global position of</param>
		/// <returns>Vector3 global object position</returns>
		public static Vector3 GetGlobalObjectPosition(Vector3 objPos)
		{
			return new Vector3((float)(-mainscript.M.mainWorld.coord.x + objPos.x), (float)(-mainscript.M.mainWorld.coord.y + objPos.y), (float)(-mainscript.M.mainWorld.coord.z + objPos.z));
		}

		/// <summary>
		/// Get object local position from global.
		/// </summary>
		/// <param name="globalPos">Current global position</param>
		/// <returns>Vector3 local object position</returns>
		public static Vector3 GetLocalObjectPosition(Vector3 globalPos)
		{
			return new Vector3((float)-(-mainscript.M.mainWorld.coord.x - globalPos.x), (float)-(-mainscript.M.mainWorld.coord.y - globalPos.y), (float)-(-mainscript.M.mainWorld.coord.z - globalPos.z));
		}
	}
}
