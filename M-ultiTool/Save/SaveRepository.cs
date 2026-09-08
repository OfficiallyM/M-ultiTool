using MultiTool.Save.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiTool.Save
{
	/// <summary>
	/// Generic CRUD over the polymorphic Save.Records list.
	/// </summary>
	internal static class SaveRepository
	{
		/// <summary>
		/// Insert or update a save record.
		/// <para>
		/// For records that spawn their own object (RequiresInstantiation), no matching is
		/// done - the first Upsert assigns a new InstanceID and adds it as-is. Pass a record
		/// with an existing InstanceID already set to update that specific spawned instance.
		/// </para>
		/// </summary>
		/// <param name="record">The record to insert or update</param>
		/// <param name="match">
		/// Predicate used to find an existing record to update. Not required for a first-time
		/// instantiated record, but required for everything else.
		/// </param>
		public static void Upsert<T>(T record, Func<T, bool> match = null) where T : SaveRecord
		{
			Save data = SaveCache.Get();

			if (record.RequiresInstantiation && string.IsNullOrEmpty(record.InstanceID))
			{
				record.InstanceID = Guid.NewGuid().ToString();
				data.Records.Add(record);
				SaveCache.Enqueue(data);
				return;
			}

			T existing = match != null ? data.Records.OfType<T>().FirstOrDefault(match) : null;
			if (existing != null)
				data.Records[data.Records.IndexOf(existing)] = record;
			else
				data.Records.Add(record);

			SaveCache.Enqueue(data);
		}

		/// <summary>
		/// Remove a matching save record, if one exists.
		/// </summary>
		/// <param name="match">Predicate used to find the record to remove</param>
		public static void Delete<T>(Func<T, bool> match) where T : SaveRecord
		{
			Save data = SaveCache.Get();

			T existing = data.Records.OfType<T>().FirstOrDefault(match);
			if (existing != null)
				data.Records.Remove(existing);

			SaveCache.Enqueue(data);
		}

		/// <summary>
		/// Get a single matching save record, or null.
		/// </summary>
		public static T Get<T>(Func<T, bool> match) where T : SaveRecord
			=> SaveCache.Get().Records.OfType<T>().FirstOrDefault(match);

		/// <summary>
		/// Get every save record of a given type.
		/// </summary>
		public static IEnumerable<T> GetAll<T>() where T : SaveRecord
			=> SaveCache.Get().Records.OfType<T>();

		/// <summary>
		/// Get every save record - of any type - matched to a given ID. Used by
		/// TriggerSaveLoad so a new record type is picked up automatically just by
		/// implementing ISaveApplier, with no separate registration step.
		/// </summary>
		public static IEnumerable<SaveRecord> GetAllForID(int id)
			=> SaveCache.Get().Records.Where(r => r.ID == id);
	}
}
