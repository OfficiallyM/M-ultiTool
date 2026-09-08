using MultiTool.Save.Records;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save
{
	/// <summary>
	/// Owns the current Save instance and batches writes rather than round-tripping
	/// serialize/deserialize on every single SaveRepository call.
	/// </summary>
	internal static class SaveCache
	{
		private static Save _cache;
		private static bool _dirty;
		private static float _lastFlushTime;

		// How long to let writes batch up before actually serializing and hitting the save plate.
		private const float _flushIntervalSeconds = 2f;

		private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		};

		// Map of record types - wire name -> Type for the read side.
		private static readonly Dictionary<string, Type> _recordTypes = new Dictionary<string, Type>
		{
			{ "poi", typeof(PoiRecord) },
			{ "glass", typeof(GlassRecord) },
			{ "material", typeof(MaterialRecord) },
			{ "scale", typeof(ScaleRecord) },
			{ "slot", typeof(SlotRecord) },
			{ "light", typeof(LightRecord) },
			{ "engineTuning", typeof(EngineTuningRecord) },
			{ "transmissionTuning", typeof(TransmissionTuningRecord) },
			{ "vehicleTuning", typeof(VehicleTuningRecord) },
			{ "wheelTuning", typeof(WheelTuningRecord) },
			{ "weight", typeof(WeightRecord) },
			{ "tank", typeof(TankRecord) },
		};

		// Derived from _recordTypes - Type -> wire name for the write side.
		private static readonly Dictionary<Type, string> _recordTypeNames =
			_recordTypes.ToDictionary(kv => kv.Value, kv => kv.Key);


		/// <summary>
		/// Get the current save data, reading and migrating it from the game save if this
		/// is the first access since load.
		/// </summary>
		public static Save Get()
		{
			if (_cache != null) return _cache;

			_cache = Unserialize();
			return _cache;
		}

		/// <summary>
		/// Replace the cached save data and mark it dirty. Does not write immediately -
		/// call Tick() from an Update loop, or Flush() to write straight away.
		/// </summary>
		public static void Enqueue(Save data)
		{
			_cache = data;
			_dirty = true;
		}

		/// <summary>
		/// Call once per frame (e.g. from MultiTool.Update()). Flushes to the save plate
		/// if there are pending writes and the flush interval has elapsed.
		/// </summary>
		public static void Tick()
		{
			if (!_dirty) return;
			if (Time.unscaledTime - _lastFlushTime < _flushIntervalSeconds) return;

			Flush();
		}

		/// <summary>
		/// Write pending changes to the save plate.
		/// </summary>
		public static void Flush()
		{
			if (!_dirty) return;

			Serialize(_cache);
			_dirty = false;
			_lastFlushTime = Time.unscaledTime;
		}

		/// <summary>
		/// Drop the in-memory cache so the next Get() re-reads from the save plate.
		/// </summary>
		public static void InvalidateCache() => _cache = null;

		private static Save Unserialize()
		{
			try
			{
				string raw = SaveUtilities.ReadWriteToGameSave();
				if (string.IsNullOrEmpty(raw))
					return new Save();

				JObject root = JObject.Parse(raw);

				bool isLegacyShape = root["Records"] == null &&
					(root["pois"] != null || root["glass"] != null || root["materials"] != null ||
					 root["scale"] != null || root["slots"] != null || root["lights"] != null);

				if (isLegacyShape)
				{
					Save migrated = SaveMigration.MigrateLegacy(raw);
					// Commit the migration immediately to ensure it persists.
					Serialize(migrated); 
					return migrated;
				}

				Save data = new Save
				{
					PlayerData = root["PlayerData"]?.ToObject<PlayerData>(),
					IsPlayerDataPerSave = root["IsPlayerDataPerSave"]?.ToObject<bool>() ?? false,
					TimeData = root["TimeData"]?.ToObject<TimeData>(),
				};

				if (root["Records"] is JArray records)
				{
					foreach (JToken token in records)
					{
						SaveRecord record = ReadRecord(token as JObject);
						if (record != null)
							data.Records.Add(record);
					}
				}

				return data;
			}
			catch (Exception ex)
			{
				Logger.Log($"Save read error - {ex}", Logger.LogLevel.Error);
				return new Save();
			}
		}

		// Unwrap a { RecordType, Data } envelope into the concrete record it describes.
		private static SaveRecord ReadRecord(JObject wrapper)
		{
			if (wrapper == null) return null;

			string recordType = wrapper["RecordType"]?.ToString();
			JObject recordData = wrapper["Data"] as JObject;
			if (recordData == null) return null;

			if (recordType == null || !_recordTypes.TryGetValue(recordType, out Type type))
			{
				Logger.Log($"Unknown save record type '{recordType}' - skipped.", Logger.LogLevel.Warning);
				return null;
			}

			return recordData.ToObject(type) as SaveRecord;
		}

		private static void Serialize(Save data)
		{
			try
			{
				JObject root = new JObject
				{
					["PlayerData"] = data.PlayerData != null ? JObject.FromObject(data.PlayerData) : null,
					["IsPlayerDataPerSave"] = data.IsPlayerDataPerSave,
					["TimeData"] = data.TimeData != null ? JObject.FromObject(data.TimeData) : null,
				};

				JArray records = new JArray();
				foreach (SaveRecord record in data.Records)
				{
					if (!_recordTypeNames.TryGetValue(record.GetType(), out string recordType))
					{
						Logger.Log($"Save write error - unknown record type '{record.GetType().Name}', skipped.", Logger.LogLevel.Error);
						continue;
					}

					records.Add(new JObject
					{
						["RecordType"] = recordType,
						["Data"] = JObject.FromObject(record, JsonSerializer.Create(_settings)),
					});
				}
				root["Records"] = records;

				SaveUtilities.ReadWriteToGameSave(root.ToString(Formatting.None));
			}
			catch (Exception ex)
			{
				Logger.Log($"Save write error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
