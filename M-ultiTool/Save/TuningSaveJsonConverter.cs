using MultiTool.UI.Tabs.VehicleConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MultiTool.Save
{
	internal class TuningSaveJsonConverter : JsonConverter<TuningSave>
	{
		private static readonly Dictionary<string, Type> _tuningTypes = new Dictionary<string, Type>
		{
			{ "engine", typeof(EngineTuning) },
			{ "transmission", typeof(TransmissionTuning) },
			{ "vehicle", typeof(VehicleTuning) },
			{ "wheel", typeof(WheelTuning) },
		};

		public override void WriteJson(JsonWriter writer, TuningSave value, JsonSerializer serializer)
		{
			JObject obj = new JObject
			{
				["Name"] = value.Name,
				["Part"] = value.Part,
				["Type"] = value.Type,
				["Car"] = value.Car,
				["Tuning"] = value.Tuning != null ? JObject.FromObject(value.Tuning, serializer) : null,
			};
			obj.WriteTo(writer);
		}

		public override TuningSave ReadJson(JsonReader reader, Type objectType, TuningSave existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject obj = JObject.Load(reader);

			TuningSave tune = new TuningSave
			{
				Name = obj["Name"]?.ToString(),
				Part = obj["Part"]?.ToString(),
				Type = obj["Type"]?.ToString(),
				Car = obj["Car"]?.ToString(),
			};

			if (tune.Type != null && _tuningTypes.TryGetValue(tune.Type, out Type tuningType) && obj["Tuning"] is JObject tuningData)
				tune.Tuning = tuningData.ToObject(tuningType, serializer) as ITuning;

			return tune;
		}
	}
}