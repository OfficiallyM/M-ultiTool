using MultiTool.Extensions;
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
			TypeNameHandling = TypeNameHandling.Auto,
			SerializationBinder = new SaveSerializationBinder(),
			Converters = { new ColorJsonConverter() },
		};

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

				return JsonConvert.DeserializeObject<Save>(raw, _settings) ?? new Save();
			}
			catch (Exception ex)
			{
				Logger.Log($"Save read error - {ex}", Logger.LogLevel.Error);
				return new Save();
			}
		}

		private static void Serialize(Save data)
		{
			try
			{
				string json = JsonConvert.SerializeObject(data, _settings);
				SaveUtilities.ReadWriteToGameSave(json);
			}
			catch (Exception ex)
			{
				Logger.Log($"Save write error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
