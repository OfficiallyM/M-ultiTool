using MultiTool.Save.Records;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiTool.Save
{
	/// <summary>
	/// Maps save record types to short, deliberate wire names instead of Json.NET's default
	/// assembly-qualified type name. Renaming or moving a record class never breaks existing
	/// save data as a result - only editing this map does.
	/// </summary>
	internal class SaveSerializationBinder : ISerializationBinder
	{
		private static readonly Dictionary<string, Type> _typesByName = new Dictionary<string, Type>
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

		private static readonly Dictionary<Type, string> _namesByType =
			_typesByName.ToDictionary(kv => kv.Value, kv => kv.Key);

		public void BindToName(Type serializedType, out string assemblyName, out string typeName)
		{
			// Never write an assembly name - keeps the JSON free of build-specific detail
			// and means BindToType below is the only thing that resolves a wire name to a type.
			assemblyName = null;
			typeName = _namesByType.TryGetValue(serializedType, out string name) ? name : serializedType.Name;
		}

		public Type BindToType(string assemblyName, string typeName)
		{
			if (_typesByName.TryGetValue(typeName, out Type type))
				return type;

			throw new JsonSerializationException($"Unknown save record type '{typeName}'. Add it to SaveSerializationBinder if this is a new record type.");
		}
	}
}
