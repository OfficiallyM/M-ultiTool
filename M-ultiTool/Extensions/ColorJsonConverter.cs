using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace MultiTool.Extensions
{
	internal class ColorJsonConverter : JsonConverter<Color>
	{
		public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("r"); writer.WriteValue(value.r);
			writer.WritePropertyName("g"); writer.WriteValue(value.g);
			writer.WritePropertyName("b"); writer.WriteValue(value.b);
			writer.WritePropertyName("a"); writer.WriteValue(value.a);
			writer.WriteEndObject();
		}

		public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var obj = JObject.Load(reader);
			return new Color(
				obj["r"]?.Value<float>() ?? 0f,
				obj["g"]?.Value<float>() ?? 0f,
				obj["b"]?.Value<float>() ?? 0f,
				obj["a"]?.Value<float>() ?? 1f);
		}
	}
}