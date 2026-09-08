using MultiTool.Save.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save
{
	internal static class SaveMigration
	{
		/// <summary>
		/// Read a pre-rewrite save (flat typed lists, DataContractJsonSerializer) and flatten
		/// it into the new Save shape (single polymorphic Records list). The caller is
		/// responsible for writing the result back out - this only maps in memory.
		/// </summary>
		/// <param name="raw">Raw save JSON in the old shape</param>
		/// <returns>Migrated save data, or an empty Save if migration fails</returns>
		public static Save MigrateLegacy(string raw)
		{
			Logger.Log($"Migrating legacy data. Raw data:\n{raw}");
			try
			{
				MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(raw));
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SaveLegacy));
				SaveLegacy legacy = serializer.ReadObject(ms) as SaveLegacy;
				ms.Close();

				if (legacy == null)
					return new Save();

				Save data = new Save
				{
					IsPlayerDataPerSave = legacy.IsPlayerDataPerSave,
				};

				if (legacy.PlayerData != null)
				{
					data.PlayerData = new PlayerData
					{
						WalkSpeed = legacy.PlayerData.WalkSpeed,
						RunSpeed = legacy.PlayerData.RunSpeed,
						JumpForce = legacy.PlayerData.JumpForce,
						PushForce = legacy.PlayerData.PushForce,
						CarryWeight = legacy.PlayerData.CarryWeight,
						PickupForce = legacy.PlayerData.PickupForce,
						ThrowForce = legacy.PlayerData.ThrowForce,
						PedalSpeed = legacy.PlayerData.PedalSpeed,
						InfiniteAmmo = legacy.PlayerData.InfiniteAmmo,
						Mass = legacy.PlayerData.Mass,
						ClickTeleport = legacy.PlayerData.ClickTeleport,
					};
				}

				if (legacy.TimeData != null)
				{
					data.TimeData = new TimeData
					{
						Timescale = legacy.TimeData.Timescale,
						DayLength = legacy.TimeData.DayLength,
						NightLength = legacy.TimeData.NightLength,
					};
				}

				Add(data, legacy.Pois, p => new PoiRecord
				{
					ID = p.ID,
					Poi = p.Poi,
					SpawnPosition = p.Position,
					SpawnRotation = p.Rotation,
					InstanceID = Guid.NewGuid().ToString(),
				});
				Add(data, legacy.Glass, g => new GlassRecord { ID = g.ID, Color = g.Color, Type = g.Type });
				Add(data, legacy.Materials, m => new MaterialRecord
				{
					ID = m.ID,
					Part = m.Part,
					Parent = m.Parent,
					IsConditionless = m.IsConditionless,
					Exact = m.Exact,
					Type = m.Type,
					Color = m.Color,
				});
				Add(data, legacy.Scale, s => new ScaleRecord { ID = s.ID, Scale = s.Scale });
				Add(data, legacy.Slots, s => new SlotRecord
				{
					ID = s.ID,
					Slot = s.Slot,
					Position = s.Position,
					ResetPosition = s.ResetPosition,
					Rotation = s.Rotation,
					ResetRotation = s.ResetRotation,
				});
				Add(data, legacy.Lights, l => new LightRecord { ID = l.ID, Name = l.Name, Color = l.Color });
				Add(data, legacy.EngineTuning, e => new EngineTuningRecord { ID = e.ID, Tuning = e.Tuning, DefaultTuning = e.DefaultTuning });
				Add(data, legacy.TransmissionTuning, e => new TransmissionTuningRecord { ID = e.ID, Tuning = e.Tuning, DefaultTuning = e.DefaultTuning });
				Add(data, legacy.VehicleTuning, e => new VehicleTuningRecord { ID = e.ID, Tuning = e.Tuning, DefaultTuning = e.DefaultTuning });
				Add(data, legacy.WheelTuning, e => new WheelTuningRecord { ID = e.ID, Tuning = e.Tuning, DefaultTuning = e.DefaultTuning });
				Add(data, legacy.Weight, w => new WeightRecord { ID = w.ID, Mass = w.Mass, DefaultMass = w.DefaultMass });
				Add(data, legacy.Tank, t => new TankRecord { ID = t.ID, Capacity = t.Capacity, DefaultCapacity = t.DefaultCapacity });

				return data;
			}
			catch (Exception ex)
			{
				Logger.Log($"Save migration error - {ex}", Logger.LogLevel.Error);
				return new Save();
			}
		}

		private static void Add<TLegacy, TRecord>(Save data, List<TLegacy> legacyList, Func<TLegacy, TRecord> map)
			where TRecord : SaveRecord
		{
			if (legacyList == null) return;

			foreach (TLegacy item in legacyList)
				data.Records.Add(map(item));
		}
	}
}
