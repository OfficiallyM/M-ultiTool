using MultiTool.Data;
using MultiTool.Save.Records;
using MultiTool.Services;
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
		private static ServiceContext _services;
		private static GlobalSave _globalData;

		public static void Bootstrap(ServiceContext services)
		{
			_services = services;
		}

		/// <summary>
		/// Read/write data to game save
		/// <para>Originally from RundensWheelPositionEditor</para>
		/// </summary>
		/// <param name="input">The string to write to the save</param>
		/// <returns>The read/written string</returns>
		internal static string ReadWriteToGameSave(string input = null)
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
		/// Insert a new POI into the save. Unlike every other record type, POIs are spawned
		/// rather than matched onto an existing object, so there's no "update" case here -
		/// call DeletePOI with the returned InstanceID and insert a new one instead.
		/// </summary>
		/// <returns>The InstanceID of the newly saved POI, needed to delete it later</returns>
		public static string InsertPOI(string poi, Vector3 position, Quaternion rotation)
		{
			PoiRecord record = new PoiRecord
			{
				Poi = poi,
				SpawnPosition = GameUtilities.GetGlobalObjectPosition(position),
				SpawnRotation = rotation,
			};

			SaveRepository.Upsert(record);
			return record.InstanceID;
		}

		/// <summary>
		/// Remove a saved POI by the InstanceID returned from InsertPOI.
		/// </summary>
		public static void DeletePOI(string instanceID)
		{
			SaveRepository.Delete<PoiRecord>(p => p.InstanceID == instanceID);
		}

		/// <summary>
		/// Update player data in save.
		/// </summary>
		public static void UpdatePlayerData(PlayerData playerData)
		{
			Save data = SaveCache.Get();
			data.PlayerData = playerData;
			SaveCache.Enqueue(data);
		}

		/// <summary>
		/// Update if player data is per save or global.
		/// </summary>
		public static void UpdateIsPlayerDataPerSave(bool perSave)
		{
			Save data = SaveCache.Get();
			data.IsPlayerDataPerSave = perSave;
			SaveCache.Enqueue(data);
		}

		/// <summary>
		/// Update time data in save.
		/// </summary>
		public static void UpdateTimeData(TimeData timeData)
		{
			Save data = SaveCache.Get();
			data.TimeData = timeData;
			SaveCache.Enqueue(data);
		}

		/// <summary>
		/// Load POIs from save and spawn them.
		/// </summary>
		/// <returns>List of newly spawned POIs</returns>
		public static List<SpawnedPOI> LoadPOIs()
		{
			List<SpawnedPOI> spawnedPOIs = new List<SpawnedPOI>();

			try
			{
				foreach (PoiRecord poi in SaveRepository.GetAll<PoiRecord>())
				{
					GameObject gameObject = _services.Database.Pois.FirstOrDefault(p => p.Obj.name == poi.Poi.Replace("(Clone)", "")).Obj;
					if (gameObject != null && poi.SpawnPosition.HasValue && poi.SpawnRotation.HasValue)
					{
						Vector3 position = GameUtilities.GetLocalObjectPosition(poi.SpawnPosition.Value);
						spawnedPOIs.Add(SpawnUtilities.Spawn(new Poi() { Obj = gameObject }, false, position, poi.SpawnRotation.Value));
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
        /// Load all save data in scene.
        /// </summary>
        public static void LoadSceneSaveData()
        {
            // Find all saveable objects.
            List<tosaveitemscript> saves = UnityEngine.Object.FindObjectsOfType<tosaveitemscript>().ToList();
            foreach (tosaveitemscript save in saves)
            {
                TriggerSaveLoad(save);
            }
        }


		/// <summary>
		/// Trigger the actual loading of the save data for a given tosaveitemscript.
		/// </summary>
		public static void TriggerSaveLoad(tosaveitemscript save)
		{
			foreach (SaveRecord record in SaveRepository.GetAllForID(save.idInSave))
			{
				if (record is ISaveApplier applier)
					applier.Apply(save);
			}
		}

		/// <summary>
		/// Load player data.
		/// </summary>
		/// <param name="defaultPlayerData">Default player data to set if saved is null</param>
		/// <returns>Loaded player data or default if it isn't saved</returns>
		public static PlayerData GetPlayerData(PlayerData defaultPlayerData)
		{
			Save data = SaveCache.Get();

			if (data.PlayerData == null)
			{
				data.PlayerData = defaultPlayerData;
				SaveCache.Enqueue(data);
			}

			return data.PlayerData;
		}

		/// <summary>
		/// Load if player data is per save or global.
		/// </summary>
		public static bool GetIsPlayerDataPerSave()
		{
			return SaveCache.Get().IsPlayerDataPerSave;
		}

		/// <summary>
		/// Get time data from save.
		/// </summary>
		public static TimeData GetTimeData()
		{
			return SaveCache.Get().TimeData;
		}

		// GlobalSave/tunes below are unchanged from the pre-rewrite Save system and still on
		// DataContractJsonSerializer until the new pattern is locked in.

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
		public static void UpdateGlobalPlayerData(PlayerData playerData)
		{
			_globalData.PlayerData = playerData;
			WriteGlobalData();
		}

		/// <summary>
		/// Load global player data.
		/// </summary>
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
		public static List<TuningSave> GetTunes()
		{
			ReadGlobalData();

			if (_globalData.Tunes == null)
				_globalData.Tunes = new List<TuningSave>();

			return _globalData.Tunes;
		}

		/// <summary>
		/// Get vehicle tunes by tune type.
		/// </summary>
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
	}
}
