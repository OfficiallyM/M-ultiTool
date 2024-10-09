using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Modules.Logger;

namespace MultiTool.Utilities
{
	/// <summary>
	/// Save data utilities.
	/// </summary>
	internal static class SaveUtilities
	{
        private static GlobalSave _globalData;

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
				if (savedatascript.s.data.items.TryGetValue((uint)Mathf.Abs(MultiTool.mod.ID.GetHashCode()), out savePrefab1) && savePrefab1.rendszam != null)
					saveRendszam = savePrefab1.rendszam;

				// Plate doesn't exist.
				if (saveRendszam == null)
				{
					// Create a new plate to store the input string in.
					tosaveitemscript component = itemdatabase.s.gplate.GetComponent<tosaveitemscript>();
					save_prefab savePrefab2 = new save_prefab(component.id, new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue), new Vector3(0.0f, 0.0f, 0.0f));
					savePrefab2.rendszam = new save_rendszam();
					saveRendszam = savePrefab2.rendszam;
					saveRendszam.S = string.Empty;
					savedatascript.s.data.items.Add((uint)Mathf.Abs(MultiTool.mod.ID.GetHashCode()), savePrefab2);
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
		private static Save UnserializeSaveData()
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
		private static void SerializeSaveData(Save data)
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
		public static int UpdatePOISaveData(POIData poi, string type = "insert")
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
						poi.position = poi.position;

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
        public static void UpdateGlass(GlassData glass)
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
        public static void UpdateMaterials(MaterialData material)
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
        public static void UpdateScale(ScaleData scale)
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
        public static void UpdateSlot(SlotData slot)
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
        /// Update light data in save.
        /// </summary>
        /// <param name="light">Light data</param>
        public static void UpdateLight(LightData light)
        {
            Save data = UnserializeSaveData();

            try
            {
                if (data.lights == null)
                    data.lights = new List<LightData>();

                LightData existing = data.lights.Where(l => l.ID == light.ID && l.name == light.name).FirstOrDefault();
                if (existing != null)
                    existing.color = light.color;
                else
                    data.lights.Add(light);
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
                if (data.engineTuning == null)
                    data.engineTuning = new List<EngineTuningData>();

                EngineTuningData existing = data.engineTuning.Where(e => e.ID == engineTuning.ID).FirstOrDefault();
                if (existing != null)
                    existing.tuning = engineTuning.tuning;
                else
                    data.engineTuning.Add(engineTuning);
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
        /// <param name="engineTuning">Engine tuning data</param>
        public static void UpdateTransmissionTuning(TransmissionTuningData transmissionTuning)
        {
            Save data = UnserializeSaveData();

            try
            {
                if (data.transmissionTuning == null)
                    data.transmissionTuning = new List<TransmissionTuningData>();

                TransmissionTuningData existing = data.transmissionTuning.Where(e => e.ID == transmissionTuning.ID).FirstOrDefault();
                if (existing != null)
                    existing.tuning = transmissionTuning.tuning;
                else
                    data.transmissionTuning.Add(transmissionTuning);
            }
            catch (Exception ex)
            {
                Logger.Log($"Transmission tuning update error - {ex}", Logger.LogLevel.Error);
            }

            SerializeSaveData(data);
        }

        /// <summary>
        /// Update transmission tuning data in save.
        /// </summary>
        /// <param name="engineTuning">Engine tuning data</param>
        public static void UpdateVehicleTuning(VehicleTuningData vehicleTuning)
        {
            Save data = UnserializeSaveData();

            try
            {
                if (data.vehicleTuning == null)
                    data.vehicleTuning = new List<VehicleTuningData>();

                VehicleTuningData existing = data.vehicleTuning.Where(e => e.ID == vehicleTuning.ID).FirstOrDefault();
                if (existing != null)
                    existing.tuning = vehicleTuning.tuning;
                else
                    data.vehicleTuning.Add(vehicleTuning);
            }
            catch (Exception ex)
            {
                Logger.Log($"Vehicle tuning update error - {ex}", Logger.LogLevel.Error);
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

            data.playerData = playerData;

            SerializeSaveData(data);
        }

        /// <summary>
        /// Update if player data is per save or global.
        /// </summary>
        /// <param name="perSave">True for per save, false for global</param>
        public static void UpdateIsPlayerDataPerSave(bool perSave)
        {
            Save data = UnserializeSaveData();

            data.isPlayerDataPerSave = perSave;

            SerializeSaveData(data);
        }

        /// <summary>
        /// Load POIs from save.
        /// </summary>
        /// <returns>List of newly spawned POIs</returns>
        //public static List<SpawnedPOI> LoadPOIs()
        //{
        //	List<POI> POIs = DatabaseUtilities.LoadPOIs();
        //	List<SpawnedPOI> spawnedPOIs = new List<SpawnedPOI>();
        //	// Load and spawn saved POIs.
        //	try
        //	{
        //		Save data = SaveUtilities.UnserializeSaveData();
        //		if (data.pois != null)
        //		{
        //			foreach (POIData poi in data.pois)
        //			{
        //				GameObject gameObject = POIs.Where(p => p.poi.name == poi.poi.Replace("(Clone)", "")).FirstOrDefault().poi;
        //				if (gameObject != null)
        //				{
        //					//Vector3 position = GameUtilities.GetLocalObjectPosition(poi.position);
        //					spawnedPOIs.Add(SpawnUtilities.Spawn(new POI() { poi = gameObject }, false, poi.position, poi.rotation));
        //				}
        //			}
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		Logger.Log($"POI load error - {ex}", Logger.LogLevel.Error);
        //	}

        //	return spawnedPOIs;
        //}

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
                LoadGlass(save, data);
                LoadMaterials(save, data);
                LoadScale(save, data);
                LoadSlots(save, data);
                LoadLights(save, data);
                LoadEngineTuning(save, data);
                LoadTransmissionTuning(save, data);
                LoadVehicleTuning(save, data);
            }
        }

        /// <summary>
        /// Load glass saved data.
        /// </summary>
        /// <param name="save">Savable object to check</param>
        /// <param name="data">Save data</param>
        public static void LoadGlass(tosaveitemscript save, Save data)
		{
            // Return early if no glass data is set.
            if (data.glass == null) return;

			foreach (GlassData glass in data.glass)
			{
			    try
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
			if (data.materials == null) return;

            foreach (MaterialData material in data.materials)
            {
			    try
			    {
                    // Check ID matches.
                    if (save.idInSave == material.ID)
                    {
                        if (material.isConditionless.HasValue && material.isConditionless.Value)
                        {
                            // Conditionless part.
                            List<MeshRenderer> meshes = new List<MeshRenderer>();

                            if (material.exact)
                            {
                                MeshRenderer mesh = GameUtilities.GetConditionlessVehiclePartByName(save.gameObject, material.part);
                                if (mesh != null)
                                    meshes.Add(mesh);
                                // Match by partial name as a failover.
                                else
                                {
                                    List<MeshRenderer> matchedMeshes = GameUtilities.GetConditionlessVehiclePartsByName(save.gameObject, material.part);
                                    if (matchedMeshes.Count > 0)
                                        meshes.AddRange(matchedMeshes);
                                }
                            }
                            else
                            {
                                List<MeshRenderer> matchedMeshes = GameUtilities.GetConditionlessVehiclePartsByName(save.gameObject, material.part);
                                if (matchedMeshes.Count > 0)
                                    meshes.AddRange(matchedMeshes);
                            }

                            foreach (MeshRenderer mesh in meshes)
                            {
                                GameUtilities.SetConditionlessPartMaterial(mesh, material.type, material.color);
                            }
                        }
                        else
                        {
                            // Standard part.
                            List<partconditionscript> parts = new List<partconditionscript>();

                            if (material.exact)
                            {
                                partconditionscript part = GameUtilities.GetVehiclePartByName(save.gameObject, material.part, false);
                                if (part != null)
                                    parts.Add(part);
                                // Match by partial name as a failover.
                                else
                                {
                                    List<partconditionscript> matchedParts = GameUtilities.GetVehiclePartsByPartialName(save.gameObject, material.part, false);
                                    if (matchedParts.Count > 0)
                                        parts.AddRange(matchedParts);
                                }
                            }
                            else
                            {
                                List<partconditionscript> matchedParts = GameUtilities.GetVehiclePartsByPartialName(save.gameObject, material.part, false);
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
			if (data.scale == null) return;

			foreach (ScaleData scale in data.scale)
			{
			    try
			    {
					// Check ID matches.
					if (save.idInSave == scale.ID)
					{
						save.gameObject.transform.localScale = scale.scale;
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
			if (data.slots == null) return;

			foreach (SlotData slot in data.slots)
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
							if (transform.name == slot.slot)
							{
								transform.localPosition = slot.position;
								transform.localRotation = slot.rotation;
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
        /// Load light data
        /// </summary>
        /// <param name="save">Savable object to check</param>
        /// <param name="data">Save data</param>
        private static void LoadLights(tosaveitemscript save, Save data)
        {
            // Return early if no light data is set.
            if (data.lights == null) return;

            foreach (LightData light in data.lights)
            {
                try
                {
                    if (save.idInSave == light.ID)
                    {
                        headlightscript headlight = null;
                        bool isInteriorLight = false;
                        if (light.name != null && light.name != string.Empty)
                        {
                            headlightscript[] lights = save.GetComponentsInChildren<headlightscript>();
                            foreach (headlightscript childLight in lights)
                            {
                                if (childLight.name.ToLower().Contains(light.name.ToLower()))
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

                        GameUtilities.SetHeadlightColor(headlight, light.color, isInteriorLight);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Light data load error - {ex}", Logger.LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Load engine tuning data
        /// </summary>
        /// <param name="save">Savable object to check</param>
        /// <param name="data">Save data</param>
        private static void LoadEngineTuning(tosaveitemscript save, Save data)
        {
            // Return early if no engine tuning data is set.
            if (data.engineTuning == null) return;

            foreach (EngineTuningData engineTuning in data.engineTuning)
            {
                try
                {
                    if (save.idInSave == engineTuning.ID)
                    {
                        GameUtilities.ApplyEngineTuning(save.GetComponent<enginescript>(), engineTuning.tuning);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Engine tuning data load error - {ex}", Logger.LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Load engine tuning data
        /// </summary>
        /// <param name="save">Savable object to check</param>
        /// <param name="data">Save data</param>
        private static void LoadTransmissionTuning(tosaveitemscript save, Save data)
        {
            // Return early if no transmission tuning data is set.
            if (data.transmissionTuning == null) return;

            foreach (TransmissionTuningData transmissionTuning in data.transmissionTuning)
            {
                try
                {
                    if (save.idInSave == transmissionTuning.ID)
                        GameUtilities.ApplyTransmissionTuning(save.GetComponent<carscript>(), transmissionTuning.tuning);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Engine tuning data load error - {ex}", Logger.LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Load vehicle tuning data
        /// </summary>
        /// <param name="save">Savable object to check</param>
        /// <param name="data">Save data</param>
        private static void LoadVehicleTuning(tosaveitemscript save, Save data)
        {
            // Return early if no vehicle tuning data is set.
            if (data.vehicleTuning == null) return;

            foreach (VehicleTuningData vehicleTuning in data.vehicleTuning)
            {
                try
                {
                    if (save.idInSave == vehicleTuning.ID)
                        GameUtilities.ApplyVehicleTuning(save.GetComponent<carscript>(), vehicleTuning.tuning);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Vehicle tuning data load error - {ex}", Logger.LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Load player data.
        /// </summary>
        /// <param name="defaultPlayerData">Default player data to set if saved is null </param>
        /// <returns>Loaded player data or default if it isn't saved</returns>
        public static PlayerData LoadPlayerData(PlayerData defaultPlayerData)
        {
            Save data = UnserializeSaveData();

            if (data.playerData == null)
            {
                data.playerData = defaultPlayerData;
                SerializeSaveData(data);
            }

            return data.playerData;
        }

        /// <summary>
        /// Load if player data is per save or global.
        /// </summary>
        /// <returns>True if player data is per save, false if global</returns>
        public static bool LoadIsPlayerDataPerSave()
        {
            Save data = UnserializeSaveData();
            return data.isPlayerDataPerSave;
        }

        /// <summary>
        /// Get slot data by ID and slot name.
        /// </summary>
        /// <param name="ID">Car save ID</param>
        /// <param name="slot">Slot name</param>
        /// <returns>SlotData if exists, otherwise null</returns>
        public static SlotData GetSlotData(uint ID, string slot)
		{
			Save data = UnserializeSaveData();

			// Return early if no slot data is set.
			if (data.slots == null) return null;

			return data.slots.Where(s => s.ID == ID && s.slot == slot).FirstOrDefault();
		}

        /// <summary>
        /// Get engine tuning by ID.
        /// </summary>
        /// <param name="ID">Engine save ID</param>
        /// <returns>EngineTuning if exists, otherwise null</returns>
        public static EngineTuning GetEngineTuning(uint ID)
        {
            Save data = UnserializeSaveData();

            return data.engineTuning?.Where(e => e.ID == ID).FirstOrDefault()?.tuning;
        }

        /// <summary>
        /// Get transmission tuning by ID.
        /// </summary>
        /// <param name="ID">Vehicle save ID</param>
        /// <returns>TransmissionTuning if exists, otherwise null</returns>
        public static TransmissionTuning GetTransmissionTuning(uint ID)
        {
            Save data = UnserializeSaveData();

            return data.transmissionTuning?.Where(e => e.ID == ID).FirstOrDefault()?.tuning;
        }

        /// <summary>
        /// Get vehicle tuning by ID.
        /// </summary>
        /// <param name="ID">Vehicle save ID</param>
        /// <returns>VehicleTuning if exists, otherwise null</returns>
        public static VehicleTuning GetVehicleTuning(uint ID)
        {
            Save data = UnserializeSaveData();

            return data.vehicleTuning?.Where(e => e.ID == ID).FirstOrDefault()?.tuning;
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
                using (FileStream file = new FileStream(Path.Combine(ModLoader.GetModConfigFolder(MultiTool.mod), "globalData.json"), FileMode.Create, FileAccess.Write))
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

                string dataPath = Path.Combine(ModLoader.GetModConfigFolder(MultiTool.mod), "GlobalData.json");
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
                Logger.Log($"Error loading config file: {ex}", Logger.LogLevel.Error);
            }
        }

        /// <summary>
        /// Update global player data. 
        /// </summary>
        /// <param name="playerData">New global player data.</param>
        public static void UpdateGlobalPlayerData(PlayerData playerData)
        {
            _globalData.playerData = playerData;
            WriteGlobalData();
        }

        /// <summary>
        /// Load global player data.
        /// </summary>
        /// <param name="defaultPlayerData">Default player data to set if saved is null </param>
        /// <returns>Loaded global player data or default if it isn't saved</returns>
        public static PlayerData LoadGlobalPlayerData(PlayerData defaultPlayerData)
        {
            ReadGlobalData();

            if (_globalData.playerData == null)
            {
                _globalData.playerData = defaultPlayerData;
                WriteGlobalData();
            }

            return _globalData.playerData;
        }
    }
}
