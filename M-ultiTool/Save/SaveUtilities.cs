using MultiTool.Database;
using MultiTool.Extensions;
using MultiTool.UI.Tabs.VehicleConfiguration;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save
{
	/// <summary>
	/// Save data utilities.
	/// </summary>
	internal static class SaveUtilities
	{
		private static GlobalSave _globalData;
		private static Save _cachedData;

		/// <summary>
		/// Read/write data to game save
		/// <para>Originally from RundensWheelPositionEditor</para>
		/// </summary>
		/// <param name="input">The string to write to the save</param>
		/// <returns>The read/written string</returns>
		private static string ReadWriteToGameSave(string input = null)
		{
			try
			{
				save_rendszam saveRendszam = null;
				save_prefab savePrefab1;

				// Attempt to find existing plate.
				if ((savedatascript.d.data.farStuff.TryGetValue(Mathf.Abs(MultiTool.ModInstance.ID.GetHashCode()), out savePrefab1) || savedatascript.d.data.nearStuff.TryGetValue(Mathf.Abs(MultiTool.ModInstance.ID.GetHashCode()), out savePrefab1)) && savePrefab1.rendszam != null)
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
					savedatascript.d.data.farStuff.Add(Mathf.Abs(MultiTool.ModInstance.ID.GetHashCode()), savePrefab2);
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

		internal static string GetRawSaveData()
		{
			return ReadWriteToGameSave();
		}

		/// <summary>
		/// Unserialize existing save data
		/// </summary>
		/// <returns>Unserialized save data</returns>
		private static Save UnserializeSaveData()
		{
			string existingString = ReadWriteToGameSave();
			if (existingString == null || existingString == string.Empty)
				return new Save();

			MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(existingString));
			DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Save));
			Save data = jsonSerializer.ReadObject(ms) as Save;
			_cachedData = data;
			return data;
		}

		/// <summary>
		/// Serialize save data and write to save
		/// </summary>
		/// <param name="data">The data to serialize</param>
		private static void SerializeSaveData(Save data)
		{
			_cachedData = data;
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
		public static int UpdatePOISaveData(POIData poi, string type = "insert")
		{
			Save data = UnserializeSaveData();

			int ID = -1;

			try
			{
				switch (type)
				{
					case "insert":
						if (data.Pois == null)
							data.Pois = new List<POIData>();

						poi.ID = data.Pois.Count;
						ID = poi.ID;

						// Save POI with global position.
						poi.Position = GameUtilities.GetGlobalObjectPosition(poi.Position);

						data.Pois.Add(poi);
						break;
					case "delete":
						POIData poiData = data.Pois.First(p => p.ID == poi.ID);
						if (poiData != null)
						{
							ID = poiData.ID;
							data.Pois.Remove(poiData);
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
		public static void UpdateGlass(GlassData glass)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.Glass == null)
					data.Glass = new List<GlassData>();

				GlassData existing = data.Glass.Where(g => g.ID == glass.ID && g.Type == glass.Type).FirstOrDefault();
				if (existing != null)
					existing.Color = glass.Color;
				else
					data.Glass.Add(glass);
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
		public static void UpdateMaterials(MaterialData material)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.Materials == null)
					data.Materials = new List<MaterialData>();

				MaterialData existing = data.Materials.Where(m => m.ID == material.ID && m.Part == material.Part && m.Parent == material.Parent).FirstOrDefault();
				if (existing != null)
				{
					// Update existing saved part.
					existing.Exact = material.Exact;
					existing.Type = material.Type;
					existing.Color = material.Color;
				}
				else
				{
					// Add a new saved part.
					data.Materials.Add(material);
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
		public static void UpdateScale(ScaleData scale)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.Scale == null)
					data.Scale = new List<ScaleData>();

				ScaleData existing = data.Scale.Where(s => s.ID == scale.ID).FirstOrDefault();
				if (existing != null)
					existing.Scale = scale.Scale;
				else
					data.Scale.Add(scale);
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
		public static void UpdateSlot(SlotData slot)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.Slots == null)
					data.Slots = new List<SlotData>();

				SlotData existing = data.Slots.Where(s => s.ID == slot.ID && s.Slot == slot.Slot).FirstOrDefault();
				if (existing != null)
				{
					existing.Position = slot.Position;
					existing.Rotation = slot.Rotation;
				}
				else
					data.Slots.Add(slot);
			}
			catch (Exception ex)
			{
				Logger.Log($"Slot update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update light data in save.
		/// </summary>
		/// <param name="light">Light data</param>
		internal static void UpdateLight(LightData light)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.Lights == null)
					data.Lights = new List<LightData>();

				LightData existing = data.Lights.Where(l => l.ID == light.ID && l.Name == light.Name).FirstOrDefault();
				if (existing != null)
					existing.Color = light.Color;
				else
					data.Lights.Add(light);
			}
			catch (Exception ex)
			{
				Logger.Log($"Light update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update engine tuning data in save.
		/// </summary>
		/// <param name="engineTuning">Engine tuning data</param>
		public static void UpdateEngineTuning(EngineTuningData engineTuning)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.EngineTuning == null)
					data.EngineTuning = new List<EngineTuningData>();

				EngineTuningData existing = data.EngineTuning.Where(e => e.ID == engineTuning.ID).FirstOrDefault();
				if (existing != null)
				{
					existing.Tuning = engineTuning.Tuning;
					if (existing.DefaultTuning == null)
						existing.DefaultTuning = engineTuning.DefaultTuning;
				}
				else
					data.EngineTuning.Add(engineTuning);
			}
			catch (Exception ex)
			{
				Logger.Log($"Engine tuning update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update transmission tuning data in save.
		/// </summary>
		/// <param name="transmissionTuning">Transmission tuning data</param>
		public static void UpdateTransmissionTuning(TransmissionTuningData transmissionTuning)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.TransmissionTuning == null)
					data.TransmissionTuning = new List<TransmissionTuningData>();

				TransmissionTuningData existing = data.TransmissionTuning.Where(e => e.ID == transmissionTuning.ID).FirstOrDefault();
				if (existing != null)
				{
					existing.Tuning = transmissionTuning.Tuning;
					if (existing.DefaultTuning == null)
						existing.DefaultTuning = transmissionTuning.DefaultTuning;
				}
				else
					data.TransmissionTuning.Add(transmissionTuning);
			}
			catch (Exception ex)
			{
				Logger.Log($"Transmission tuning update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update vehicle tuning data in save.
		/// </summary>
		/// <param name="vehicleTuning">Vehicle tuning data</param>
		public static void UpdateVehicleTuning(VehicleTuningData vehicleTuning)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.VehicleTuning == null)
					data.VehicleTuning = new List<VehicleTuningData>();

				VehicleTuningData existing = data.VehicleTuning.Where(e => e.ID == vehicleTuning.ID).FirstOrDefault();
				if (existing != null)
				{
					existing.Tuning = vehicleTuning.Tuning;
					if (existing.DefaultTuning == null)
						existing.DefaultTuning = vehicleTuning.DefaultTuning;
				}
				else
					data.VehicleTuning.Add(vehicleTuning);
			}
			catch (Exception ex)
			{
				Logger.Log($"Vehicle tuning update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update wheel tuning data in save.
		/// </summary>
		/// <param name="wheelTuning">Wheel tuning data</param>
		public static void UpdateWheelTuning(WheelTuningData wheelTuning)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.WheelTuning == null)
					data.WheelTuning = new List<WheelTuningData>();

				WheelTuningData existing = data.WheelTuning.Where(e => e.ID == wheelTuning.ID).FirstOrDefault();
				if (existing != null)
				{
					existing.Tuning = wheelTuning.Tuning;
					if (existing.DefaultTuning == null)
						existing.DefaultTuning = wheelTuning.DefaultTuning;
				}
				else
					data.WheelTuning.Add(wheelTuning);
			}
			catch (Exception ex)
			{
				Logger.Log($"Wheel tuning update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update weight data in save.
		/// </summary>
		/// <param name="weight">Weight data</param>
		public static void UpdateWeight(WeightData weight)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.Weight == null)
					data.Weight = new List<WeightData>();

				WeightData existing = data.Weight.Where(s => s.ID == weight.ID).FirstOrDefault();
				if (existing != null)
				{
					// Only set default if we don't already have one set.
					if (existing.DefaultMass == 0)
						existing.DefaultMass = weight.DefaultMass;

					existing.Mass = weight.Mass;
				}
				else
					data.Weight.Add(weight);
			}
			catch (Exception ex)
			{
				Logger.Log($"Weight update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update tank data in save.
		/// </summary>
		/// <param name="tank">Tank data</param>
		public static void UpdateTank(TankData tank)
		{
			Save data = UnserializeSaveData();

			try
			{
				if (data.Tank == null)
					data.Tank = new List<TankData>();

				TankData existing = data.Tank.Where(s => s.ID == tank.ID).FirstOrDefault();
				if (existing != null)
				{
					// Only set default if we don't already have one set.
					if (existing.DefaultCapacity == 0)
						existing.DefaultCapacity = tank.DefaultCapacity;

					existing.Capacity = tank.Capacity;
				}
				else
					data.Tank.Add(tank);
			}
			catch (Exception ex)
			{
				Logger.Log($"Tank update error - {ex}", Logger.LogLevel.Error);
			}

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update player data in save.
		/// </summary>
		/// <param name="playerData">New player data</param>
		public static void UpdatePlayerData(PlayerData playerData)
		{
			Save data = UnserializeSaveData();

			data.PlayerData = playerData;

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update if player data is per save or global.
		/// </summary>
		/// <param name="perSave">True for per save, false for global</param>
		public static void UpdateIsPlayerDataPerSave(bool perSave)
		{
			Save data = UnserializeSaveData();

			data.IsPlayerDataPerSave = perSave;

			SerializeSaveData(data);
		}

		/// <summary>
		/// Update time data in save.
		/// </summary>
		/// <param name="timeData">New time data</param>
		public static void UpdateTimeData(TimeData timeData)
		{
			Save data = UnserializeSaveData();
			data.TimeData = timeData;
			SerializeSaveData(data);
		}

		/// <summary>
		/// Load POIs from save.
		/// </summary>
		/// <returns>List of newly spawned POIs</returns>
		public static List<SpawnedPOI> LoadPOIs()
		{
			List<POI> POIs = DatabaseUtilities.LoadPOIs();
			List<SpawnedPOI> spawnedPOIs = new List<SpawnedPOI>();
			// Load and spawn saved POIs.
			try
			{
				Save data = UnserializeSaveData();
				if (data.Pois != null)
				{
					foreach (POIData poi in data.Pois)
					{
						GameObject gameObject = POIs.Where(p => p.Poi.name == poi.Poi.Replace("(Clone)", "")).FirstOrDefault().Poi;
						if (gameObject != null)
						{
							Vector3 position = GameUtilities.GetLocalObjectPosition(poi.Position);
							spawnedPOIs.Add(SpawnUtilities.Spawn(new POI() { Poi = gameObject }, false, position, poi.Rotation));
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
		/// Load all save data.
		/// </summary>
		public static void LoadSaveData()
		{
			Save data = UnserializeSaveData();

			// Find all saveable objects.
			List<tosaveitemscript> saves = UnityEngine.Object.FindObjectsOfType<tosaveitemscript>().ToList();
			foreach (tosaveitemscript save in saves)
			{
				TriggerSaveLoad(save, data);
			}
		}

		/// <summary>
		/// Trigger the actual loading of the save data for a given tosaveitemscript.
		/// </summary>
		/// <param name="save">tosaveitemscript of the object</param>
		/// <param name="data">Fully loaded save data or null to use cached data</param>
		public static void TriggerSaveLoad(tosaveitemscript save, Save data = null)
		{
			if (data == null)
				data = _cachedData;

			LoadGlass(save, data);
			LoadMaterials(save, data);
			LoadScale(save, data);
			LoadSlots(save, data);
			LoadLights(save, data);
			LoadEngineTuning(save, data);
			LoadTransmissionTuning(save, data);
			LoadVehicleTuning(save, data);
			LoadWheelTuning(save, data);
			LoadWeight(save, data);
			LoadTank(save, data);
		}

		/// <summary>
		/// Load glass saved data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		public static void LoadGlass(tosaveitemscript save, Save data)
		{
			// Return early if no glass data is set.
			if (data.Glass == null) return;

			foreach (GlassData glass in data.Glass)
			{
				try
				{
					// Check ID matches.
					if (save.idInSave == glass.ID)
					{
						switch (glass.Type)
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
											meshRenderer.material.color = glass.Color;
											break;

										// Inner glass.
										case "GlassNoReflection":
											// Use a more transparent version of the selected colour
											// for the inner glass to ensure it's still see-through.
											Color innerColor = glass.Color;
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
									meshRenderer.material.color = glass.Color;
								}
								break;
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Glass load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load material save data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadMaterials(tosaveitemscript save, Save data)
		{
			// Return early if no material data is set.
			if (data.Materials == null) return;

			foreach (MaterialData material in data.Materials)
			{
				try
				{
					// Check ID matches.
					if (save.idInSave == material.ID)
					{
						if (material.IsConditionless.HasValue && material.IsConditionless.Value)
						{
							// Conditionless parts are always matched by exact name.
							MeshRenderer mesh = GameUtilities.GetConditionlessVehiclePartByName(save.gameObject, material.Part);
							GameUtilities.SetConditionlessPartMaterial(mesh, material.Type, material.Color);
						}
						else
						{
							// Standard part.
							List<partconditionscript> parts = new List<partconditionscript>();

							if (material.Exact)
							{
								partconditionscript part = GameUtilities.GetVehiclePartByName(save.gameObject, material.Part, false);
								if (part != null)
									parts.Add(part);
								// Match by partial name as a failover.
								else
								{
									List<partconditionscript> matchedParts = GameUtilities.GetVehiclePartsByPartialName(save.gameObject, material.Part, false);
									if (matchedParts.Count > 0)
										parts.AddRange(matchedParts);
								}
							}
							else
							{
								List<partconditionscript> matchedParts = GameUtilities.GetVehiclePartsByPartialName(save.gameObject, material.Part, false);
								if (matchedParts.Count > 0)
									parts.AddRange(matchedParts);
							}

							foreach (partconditionscript part in parts)
							{
								// Skip any parts where the parent doesn't match.
								if (material.Parent != null)
									if (material.Parent != SanitiseName(part.transform.parent?.name ?? part.name)) continue;

								GameUtilities.SetPartMaterial(part, material.Type, material.Color);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Material data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load scale data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadScale(tosaveitemscript save, Save data)
		{
			// Return early if no scale data is set.
			if (data.Scale == null) return;

			foreach (ScaleData scale in data.Scale)
			{
				try
				{
					// Check ID matches.
					if (save.idInSave == scale.ID)
					{
						save.gameObject.transform.localScale = scale.Scale;
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Scale data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load slot data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadSlots(tosaveitemscript save, Save data)
		{
			// Return early if no slot data is set.
			if (data.Slots == null) return;

			foreach (SlotData slot in data.Slots)
			{
				try
				{
					// Check ID matches.
					if (save.idInSave == slot.ID)
					{
						// Find the child part.
						foreach (Transform transform in save.GetComponentsInChildren<Transform>())
						{
							// Apply position and rotation changes.
							if (transform.name == slot.Slot)
							{
								transform.localPosition = slot.Position;
								transform.localRotation = slot.Rotation;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Slot data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load light data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadLights(tosaveitemscript save, Save data)
		{
			// Return early if no light data is set.
			if (data.Lights == null) return;

			foreach (LightData light in data.Lights)
			{
				try
				{
					if (save.idInSave == light.ID)
					{
						headlightscript headlight = null;
						bool isInteriorLight = false;
						if (light.Name != null && light.Name != string.Empty)
						{
							headlightscript[] lights = save.GetComponentsInChildren<headlightscript>();
							foreach (headlightscript childLight in lights)
							{
								if (childLight.name.ToLower().Contains(light.Name.ToLower()))
									headlight = childLight;
							}
							isInteriorLight = true;
						}
						else
						{
							headlight = save.GetComponent<headlightscript>();
						}

						// Unable to find headlight, skip.
						if (headlight == null) continue;

						GameUtilities.SetHeadlightColor(headlight, light.Color, isInteriorLight);
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Light data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load engine tuning data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadEngineTuning(tosaveitemscript save, Save data)
		{
			// Return early if no engine tuning data is set.
			if (data.EngineTuning == null) return;

			foreach (EngineTuningData engineTuning in data.EngineTuning)
			{
				try
				{
					if (save.idInSave == engineTuning.ID)
					{
						GameUtilities.ApplyEngineTuning(save.GetComponent<enginescript>(), engineTuning.Tuning);
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Engine tuning data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load transmission tuning data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadTransmissionTuning(tosaveitemscript save, Save data)
		{
			// Return early if no transmission tuning data is set.
			if (data.TransmissionTuning == null) return;

			foreach (TransmissionTuningData transmissionTuning in data.TransmissionTuning)
			{
				try
				{
					if (save.idInSave == transmissionTuning.ID)
						GameUtilities.ApplyTransmissionTuning(save.GetComponent<carscript>(), transmissionTuning.Tuning);
				}
				catch (Exception ex)
				{
					Logger.Log($"Engine tuning data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load vehicle tuning data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadVehicleTuning(tosaveitemscript save, Save data)
		{
			// Return early if no vehicle tuning data is set.
			if (data.VehicleTuning == null) return;

			foreach (VehicleTuningData vehicleTuning in data.VehicleTuning)
			{
				try
				{
					if (save.idInSave == vehicleTuning.ID)
						GameUtilities.ApplyVehicleTuning(save.GetComponent<carscript>(), vehicleTuning.Tuning);
				}
				catch (Exception ex)
				{
					Logger.Log($"Vehicle tuning data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load wheel tuning data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadWheelTuning(tosaveitemscript save, Save data)
		{
			// Return early if no vehicle tuning data is set.
			if (data.WheelTuning == null) return;

			foreach (WheelTuningData wheelTuning in data.WheelTuning)
			{
				try
				{
					if (save.idInSave == wheelTuning.ID)
					{
						if (wheelTuning.Tuning.Wheels == null || wheelTuning.Tuning.Wheels.Count == 0) continue;

						GameUtilities.RemapWheelTuning(save, wheelTuning.Tuning);
						GameUtilities.ApplyWheelTuning(wheelTuning.Tuning);
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Wheel tuning data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load weight data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadWeight(tosaveitemscript save, Save data)
		{
			// Return early if no weight data is set.
			if (data.Weight == null) return;

			foreach (WeightData weight in data.Weight)
			{
				try
				{
					// Check ID matches.
					if (save.idInSave == weight.ID)
					{
						massScript mass = save.GetComponent<massScript>();
						mass.SetMass(weight.Mass);
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Weight data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load tank data.
		/// </summary>
		/// <param name="save">Savable object to check</param>
		/// <param name="data">Save data</param>
		private static void LoadTank(tosaveitemscript save, Save data)
		{
			// Return early if no tank data is set.
			if (data.Tank == null) return;

			foreach (TankData tankData in data.Tank)
			{
				try
				{
					// Check ID matches.
					if (save.idInSave == tankData.ID)
					{
						tankscript tank = null;
						carscript car = save.GetComponent<carscript>();
						// Support for car fuel tanks.
						if (car != null)
							tank = car.Tank;
						else
							tank = save.GetComponentInChildren<tankscript>();

						if (tank != null)
							tank.F.maxC = tankData.Capacity;
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Tank data load error - {ex}", Logger.LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Load player data.
		/// </summary>
		/// <param name="defaultPlayerData">Default player data to set if saved is null</param>
		/// <returns>Loaded player data or default if it isn't saved</returns>
		public static PlayerData LoadPlayerData(PlayerData defaultPlayerData)
		{
			Save data = UnserializeSaveData();

			if (data.PlayerData == null)
			{
				data.PlayerData = defaultPlayerData;
				SerializeSaveData(data);
			}

			return data.PlayerData;
		}

		/// <summary>
		/// Load if player data is per save or global.
		/// </summary>
		/// <returns>True if player data is per save, false if global</returns>
		public static bool LoadIsPlayerDataPerSave()
		{
			Save data = UnserializeSaveData();
			return data.IsPlayerDataPerSave;
		}

		/// <summary>
		/// Get time data from save.
		/// </summary>
		/// <returns>TimeData or null if nothing is saved</returns>
		public static TimeData GetTimeData()
		{
			Save data = UnserializeSaveData();
			return data.TimeData;
		}

		/// <summary>
		/// Get slot data by ID and slot name.
		/// </summary>
		/// <param name="ID">Car save ID</param>
		/// <param name="slot">Slot name</param>
		/// <returns>SlotData if exists, otherwise null</returns>
		public static SlotData GetSlotData(int ID, string slot)
		{
			Save data = UnserializeSaveData();

			// Return early if no slot data is set.
			if (data.Slots == null) return null;

			return data.Slots.Where(s => s.ID == ID && s.Slot == slot).FirstOrDefault();
		}

		/// <summary>
		/// Get engine tuning by ID.
		/// </summary>
		/// <param name="ID">Engine save ID</param>
		/// <returns>EngineTuning if exists, otherwise null</returns>
		public static EngineTuning GetEngineTuning(int ID)
		{
			Save data = UnserializeSaveData();

			return data.EngineTuning?.Where(e => e.ID == ID).FirstOrDefault()?.Tuning;
		}

		/// <summary>
		/// Get default engine tuning by ID.
		/// </summary>
		/// <param name="ID">Engine save ID</param>
		/// <returns>EngineTuning if exists, otherwise null</returns>
		public static EngineTuning GetDefaultEngineTuning(int ID)
		{
			Save data = UnserializeSaveData();

			return data.EngineTuning?.Where(e => e.ID == ID).FirstOrDefault()?.DefaultTuning;
		}

		/// <summary>
		/// Get transmission tuning by ID.
		/// </summary>
		/// <param name="ID">Vehicle save ID</param>
		/// <returns>TransmissionTuning if exists, otherwise null</returns>
		public static TransmissionTuning GetTransmissionTuning(int ID)
		{
			Save data = UnserializeSaveData();

			return data.TransmissionTuning?.Where(e => e.ID == ID).FirstOrDefault()?.Tuning;
		}

		/// <summary>
		/// Get default transmission tuning by ID.
		/// </summary>
		/// <param name="ID">Vehicle save ID</param>
		/// <returns>TransmissionTuning if exists, otherwise null</returns>
		public static TransmissionTuning GetDefaultTransmissionTuning(int ID)
		{
			Save data = UnserializeSaveData();

			return data.TransmissionTuning?.Where(e => e.ID == ID).FirstOrDefault()?.DefaultTuning;
		}

		/// <summary>
		/// Get vehicle tuning by ID.
		/// </summary>
		/// <param name="ID">Vehicle save ID</param>
		/// <returns>VehicleTuning if exists, otherwise null</returns>
		public static VehicleTuning GetVehicleTuning(int ID)
		{
			Save data = UnserializeSaveData();

			return data.VehicleTuning?.Where(e => e.ID == ID).FirstOrDefault()?.Tuning;
		}

		/// <summary>
		/// Get default vehicle tuning by ID.
		/// </summary>
		/// <param name="ID">Vehicle save ID</param>
		/// <returns>VehicleTuning if exists, otherwise null</returns>
		public static VehicleTuning GetDefaultVehicleTuning(int ID)
		{
			Save data = UnserializeSaveData();

			return data.VehicleTuning?.Where(e => e.ID == ID).FirstOrDefault()?.DefaultTuning;
		}

		/// <summary>
		/// Get wheel tuning by ID.
		/// </summary>
		/// <param name="save">Vehicle tosaveitemscript</param>
		/// <returns>WheelTuning if exists, otherwise null</returns>
		public static WheelTuning GetWheelTuning(tosaveitemscript save)
		{
			Save data = UnserializeSaveData();

			WheelTuning tuning = data.WheelTuning?.Where(e => e.ID == save.idInSave).FirstOrDefault()?.Tuning;
			GameUtilities.RemapWheelTuning(save, tuning);
			return tuning;
		}

		/// <summary>
		/// Get default wheel tuning by ID.
		/// </summary>
		/// <param name="save">Vehicle tosaveitemscript</param>
		/// <returns>WheelTuning if exists, otherwise null</returns>
		public static WheelTuning GetDefaultWheelTuning(tosaveitemscript save)
		{
			Save data = UnserializeSaveData();

			WheelTuning tuning = data.WheelTuning?.Where(e => e.ID == save.idInSave).FirstOrDefault()?.DefaultTuning;
			return tuning;
		}

		/// <summary>
		/// Get weight data by ID.
		/// </summary>
		/// <param name="ID">Object save ID</param>
		/// <returns>WeightData if exists, otherwise null</returns>
		public static WeightData GetWeight(int ID)
		{
			Save data = UnserializeSaveData();

			return data.Weight?.Where(e => e.ID == ID).FirstOrDefault();
		}

		/// <summary>
		/// Get tank data by ID.
		/// </summary>
		/// <param name="ID">Object save ID</param>
		/// <returns>TankData if exists, otherwise null</returns>
		public static TankData GetTank(int ID)
		{
			Save data = UnserializeSaveData();

			return data.Tank?.Where(e => e.ID == ID).FirstOrDefault();
		}

		/// <summary>
		/// Write the global save data to the JSON file.
		/// </summary>
		private static void WriteGlobalData()
		{
			try
			{
				MemoryStream ms = new MemoryStream();
				DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GlobalSave));
				jsonSerializer.WriteObject(ms, _globalData);
				using (FileStream file = new FileStream(Path.Combine(ModLoader.GetModConfigFolder(MultiTool.ModInstance), "globalData.json"), FileMode.Create, FileAccess.Write))
				{
					ms.WriteTo(file);
					ms.Dispose();
				}

			}
			catch (Exception ex)
			{
				Logger.Log($"Config write error: {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Read the global save data from the JSON file.
		/// </summary>
		private static void ReadGlobalData()
		{
			// Attempt to load the config file.
			try
			{
				// Config already loaded, return early.
				if (_globalData == new GlobalSave()) return;
				if (_globalData == null)
					_globalData = new GlobalSave();

				string dataPath = Path.Combine(ModLoader.GetModConfigFolder(MultiTool.ModInstance), "GlobalData.json");
				if (File.Exists(dataPath))
				{
					string json = File.ReadAllText(dataPath);
					MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GlobalSave));
					_globalData = jsonSerializer.ReadObject(ms) as GlobalSave;
					ms.Close();
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Error loading global save data: {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Update global player data. 
		/// </summary>
		/// <param name="playerData">New global player data.</param>
		public static void UpdateGlobalPlayerData(PlayerData playerData)
		{
			_globalData.PlayerData = playerData;
			WriteGlobalData();
		}

		/// <summary>
		/// Load global player data.
		/// </summary>
		/// <param name="defaultPlayerData">Default player data to set if saved is null</param>
		/// <returns>Loaded global player data or default if it isn't saved</returns>
		public static PlayerData LoadGlobalPlayerData(PlayerData defaultPlayerData)
		{
			ReadGlobalData();

			if (_globalData.PlayerData == null)
			{
				_globalData.PlayerData = defaultPlayerData;
				WriteGlobalData();
			}

			return _globalData.PlayerData;
		}

		/// <summary>
		/// Save vehicle tune.
		/// </summary>
		/// <param name="tune">Vehicle tune to save</param>
		public static void AddTune(TuningSave tune)
		{
			ReadGlobalData();

			if (_globalData.Tunes == null)
				_globalData.Tunes = new List<TuningSave>();

			_globalData.Tunes.Add(tune);
			WriteGlobalData();
		}

		/// <summary>
		/// Remove a saved vehicle tune.
		/// </summary>
		/// <param name="tune">Vehicle tune to remove</param>
		public static void RemoveTune(TuningSave tune)
		{
			ReadGlobalData();

			if (_globalData.Tunes == null) return;

			_globalData.Tunes.Remove(tune);
			WriteGlobalData();
		}

		/// <summary>
		/// Get all saved vehicle tunes.
		/// </summary>
		/// <returns>List of saved vehicle tunes.</returns>
		public static List<TuningSave> GetTunes()
		{
			ReadGlobalData();

			if (_globalData.Tunes == null)
				_globalData.Tunes = new List<TuningSave>();

			return _globalData.Tunes;
		}

		/// <summary>
		/// Get vehiclde tunes by tune type.
		/// </summary>
		/// <param name="type">Tune type</param>
		/// <returns>List of vehicle tunes</returns>
		public static List<TuningSave> GetTunesByType(string type)
		{
			List<TuningSave> tunes = new List<TuningSave>();

			foreach (TuningSave tune in GetTunes())
			{
				if (tune.Type == type)
					tunes.Add(tune);
			}

			return tunes;
		}

		/// <summary>
		/// Sanitise name for data storage.
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>Sanitised name</returns>
		public static string SanitiseName(string name)
		{
			name = name.Replace("(Clone)", string.Empty);
			string last = name.ToLower().Substring(Math.Max(0, name.Length - 4));
			if (last == "full")
				name = name.Remove(name.Length - 4);
			name = name.Trim();

			return name;
		}
	}
}
