using MultiTool.Core;
using MultiTool.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Utilities
{
	/// <summary>
	/// Save data utilities.
	/// </summary>
	public static class SaveUtilities
	{
		/// <summary>
		/// Read/write data to game save
		/// <para>Originally from RundensWheelPositionEditor</para>
		/// </summary>
		/// <param name="input">The string to write to the save</param>
		/// <returns>The read/written string</returns>
		public static string ReadWriteToGameSave(string input = null)
		{
			try
			{
				save_rendszam saveRendszam = null;
				save_prefab savePrefab1;

				save_rendszam spawnerSaveRendszam = null;

				// Attempt to find existing plate.
				if ((savedatascript.d.data.farStuff.TryGetValue(Mathf.Abs(Meta.ID.GetHashCode()), out savePrefab1) || savedatascript.d.data.nearStuff.TryGetValue(Mathf.Abs(Meta.ID.GetHashCode()), out savePrefab1)) && savePrefab1.rendszam != null)
					saveRendszam = savePrefab1.rendszam;

				// Attempt to find old SpawnerTLD plate.
				if ((savedatascript.d.data.farStuff.TryGetValue(Mathf.Abs("SpawnerTLD".GetHashCode()), out savePrefab1) || savedatascript.d.data.nearStuff.TryGetValue(Mathf.Abs("SpawnerTLD".GetHashCode()), out savePrefab1)) && savePrefab1.rendszam != null)
					spawnerSaveRendszam = savePrefab1.rendszam;

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

				// Copy old spawner data to new plate.
				if (spawnerSaveRendszam != null)
				{
					saveRendszam.S = spawnerSaveRendszam.S;
					// Delete old spawner plate.
					savedatascript.d.data.farStuff.Remove(Mathf.Abs("SpawnerTLD".GetHashCode()));
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
		internal static Save UnserializeSaveData()
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
		internal static void SerializeSaveData(Save data)
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
		internal static int UpdatePOISaveData(POIData poi, string type = "insert")
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

						// Save POI with global position.
						poi.position = GameUtilities.GetGlobalObjectPosition(poi.position);

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
		/// <param name="glass">Glass data</param>
		internal static void UpdateGlass(GlassData glass)
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
		/// Update material data in save.
		/// </summary>
		/// <param name="material">Material data</param>
		internal static void UpdateMaterials(MaterialData material)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.materials == null)
					data.materials = new List<MaterialData>();

				MaterialData existing = data.materials.Where(m => m.ID == material.ID && m.part == material.part).FirstOrDefault();
				if (existing != null)
				{
					// Update existing saved part.
					existing.exact = material.exact;
					existing.type = material.type;
					existing.color = material.color;
				}
				else
				{
					// Add a new saved part.
					data.materials.Add(material);
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Material update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update scale data in save.
		/// </summary>
		/// <param name="scale">Scale data</param>
		internal static void UpdateScale(ScaleData scale)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.scale == null)
					data.scale = new List<ScaleData>();

				ScaleData existing = data.scale.Where(s => s.ID == scale.ID).FirstOrDefault();
				if (existing != null)
					existing.scale = scale.scale;
				else
					data.scale.Add(scale);
			}
			catch (Exception ex)
			{
				Logger.Log($"Scale update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update slot data in save.
		/// </summary>
		/// <param name="slot">Slot data</param>
		internal static void UpdateSlot(SlotData slot)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.slots == null)
					data.slots = new List<SlotData>();

				SlotData existing = data.slots.Where(s => s.ID == slot.ID && s.slot == slot.slot).FirstOrDefault();
				if (existing != null)
				{
					existing.position = slot.position;
					existing.rotation = slot.rotation;
				}
				else
					data.slots.Add(slot);
			}
			catch (Exception ex)
			{
				Logger.Log($"Slot update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Load POIs from save.
		/// </summary>
		/// <returns>List of newly spawned POIs</returns>
		internal static List<SpawnedPOI> LoadPOIs()
		{
			List<POI> POIs = DatabaseUtilities.LoadPOIs();
			List<SpawnedPOI> spawnedPOIs = new List<SpawnedPOI>();
			// Load and spawn saved POIs.
			try
			{
				Save data = SaveUtilities.UnserializeSaveData();
				if (data.pois != null)
				{
					foreach (POIData poi in data.pois)
					{
						GameObject gameObject = POIs.Where(p => p.poi.name == poi.poi.Replace("(Clone)", "")).FirstOrDefault().poi;
						if (gameObject != null)
						{
							Vector3 position = GameUtilities.GetLocalObjectPosition(poi.position);
							spawnedPOIs.Add(SpawnUtilities.Spawn(new POI() { poi = gameObject }, false, position, poi.rotation));
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"POI load error - {ex}", Logger.LogLevel.Error);
			}

			return spawnedPOIs;
		}

		/// <summary>
		/// Load glass saved data.
		/// </summary>
		internal static void LoadGlass()
		{
			// Load glass data.
			try
			{
				Save data = UnserializeSaveData();
				if (data.glass != null)
				{
					foreach (GlassData glass in data.glass)
					{
						// Find all saveable objects.
						List<tosaveitemscript> saves = UnityEngine.Object.FindObjectsOfType<tosaveitemscript>().ToList();
						foreach (tosaveitemscript save in saves)
						{
							// Check ID matches.
							if (save.idInSave == glass.ID)
							{
								switch (glass.type)
								{
									case "windows":
										// Set window colour.
										List<MeshRenderer> renderers = save.gameObject.GetComponentsInChildren<MeshRenderer>().ToList();
										foreach (MeshRenderer meshRenderer in renderers)
										{
											string materialName = meshRenderer.material.name.Replace(" (Instance)", "");
											switch (materialName)
											{
												// Outer glass.
												case "Glass":
													// Use selected colour.
													meshRenderer.material.color = glass.color;
													break;

												// Inner glass.
												case "GlassNoReflection":
													// Use a more transparent version of the selected colour
													// for the inner glass to ensure it's still see-through.
													Color innerColor = glass.color;
													if (innerColor.a > 0.2f)
														innerColor.a = 0.2f;
													meshRenderer.material.color = innerColor;
													break;
											}
										}
										break;
									case "sunroof":
										// Set sunroof colour.
										GameObject car = save.gameObject;
										Transform sunRoofSlot = car.transform.FindRecursive("SunRoofSlot");
										Transform outerGlass = sunRoofSlot.FindRecursive("sunroof outer glass", exact: false);
										if (outerGlass != null)
										{
											MeshRenderer meshRenderer = outerGlass.GetComponent<MeshRenderer>();
											meshRenderer.material.color = glass.color;
										}
										break;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Glass load error - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Load material save data.
		/// </summary>
		internal static void LoadMaterials()
		{
			try
			{
				Save data = UnserializeSaveData();
				// Return early if no material data is set.
				if (data.materials == null) return;

				foreach (MaterialData material in data.materials)
				{
					// Find all saveable objects.
					List<tosaveitemscript> saves = UnityEngine.Object.FindObjectsOfType<tosaveitemscript>().ToList();
					foreach (tosaveitemscript save in saves)
					{
						// Check ID matches.
						if (save.idInSave == material.ID)
						{
							List<partconditionscript> parts = new List<partconditionscript>();

							if (material.exact)
							{
								partconditionscript part = GameUtilities.GetVehiclePartByName(save.gameObject, material.part);
								if (part != null)
									parts.Add(part);
								// Match by partial name as a failover.
								else
								{
									List<partconditionscript> matchedParts = GameUtilities.GetVehiclePartsByPartialName(save.gameObject, material.part);
									if (matchedParts.Count > 0)
										parts.AddRange(matchedParts);
								}
							}
							else
							{
								List<partconditionscript> matchedParts = GameUtilities.GetVehiclePartsByPartialName(save.gameObject, material.part);
								if (matchedParts.Count > 0)
									parts.AddRange(matchedParts);
							}

							foreach (partconditionscript part in parts)
							{
								GameUtilities.SetPartMaterial(part, material.type, material.color);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Material data load error - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Load scale data.
		/// </summary>
		internal static void LoadScale()
		{
			try
			{
				Save data = UnserializeSaveData();
				// Return early if no scale data is set.
				if (data.scale == null) return;

				foreach (ScaleData scale in data.scale)
				{
					// Find all saveable objects.
					List<tosaveitemscript> saves = UnityEngine.Object.FindObjectsOfType<tosaveitemscript>().ToList();
					foreach (tosaveitemscript save in saves)
					{
						// Check ID matches.
						if (save.idInSave == scale.ID)
						{
							save.gameObject.transform.localScale = scale.scale;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Scale data load error - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Load slot data.
		/// </summary>
		internal static void LoadSlots()
		{
			try
			{
				Save data = UnserializeSaveData();
				// Return early if no slot data is set.
				if (data.slots == null) return;

				foreach (SlotData slot in data.slots)
				{
					// Find all saveable objects.
					List<tosaveitemscript> saves = UnityEngine.Object.FindObjectsOfType<tosaveitemscript>().ToList();
					foreach (tosaveitemscript save in saves)
					{
						// Check ID matches.
						if (save.idInSave == slot.ID)
						{
							// Find the child part.
							foreach (Transform transform in save.GetComponentsInChildren<Transform>())
							{
								// Apply position and rotation changes.
								if (transform.name == slot.slot)
								{
									transform.localPosition = slot.position;
									transform.localRotation = slot.rotation;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Slot data load error - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Get slot data by ID and slot name.
		/// </summary>
		/// <param name="ID">Car save ID</param>
		/// <param name="slot">Slot name</param>
		/// <returns>SlotData if exists, otherwise null</returns>
		internal static SlotData GetSlotData(int ID, string slot)
		{
			Save data = UnserializeSaveData();

			// Return early if no slot data is set.
			if (data.slots == null) return null;

			return data.slots.Where(s => s.ID == ID && s.slot == slot).FirstOrDefault();
		}
	}
}
