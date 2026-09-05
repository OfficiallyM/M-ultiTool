using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using static MultiTool.Services.Keybinds;

namespace MultiTool.Config
{
	internal class ConfigSerializable
	{
		public string Version { get; set; }
		public List<Key> Keybinds { get; set; } = new List<Key>();
		public float ScrollWidth { get; set; } = 10f;
		public int Accessibility { get; set; }
		public bool AccessibilityModeAffectsColor { get; set; } = true;
		public float NoclipFastMoveFactor { get; set; } = 10f;
		public List<Color> Palette { get; set; } = new List<Color>();
		public Color BasicColliderColor { get; set; } = new Color(1f, 0.0f, 0.0f, 0.8f);
		public Color TriggerColliderColor { get; set; } = new Color(0.0f, 1f, 0.0f, 0.8f);
		public Color InteriorColliderColor { get; set; } = new Color(0f, 0f, 1f, 0.8f);
		public string Theme { get; set; } = "Greyscale";
	}

	[DataContract]
	internal class ConfigSerializableLegacy
	{
		[DataMember(Name = "version")] public string Version { get; set; }
		[DataMember(Name = "keybinds")] public List<Key> Keybinds { get; set; }
		[DataMember(Name = "scrollWidth")] public float ScrollWidth { get; set; }
		[DataMember(Name = "accessibility")] public int Accessibility { get; set; }
		[DataMember(Name = "accessibilityModeAffectsColor")] public bool? AccessibilityModeAffectsColor { get; set; }
		[DataMember(Name = "noclipFastMoveFactor")] public float NoclipFastMoveFactor { get; set; }
		[DataMember(Name = "palette")] public List<Color> Palette { get; set; }
		[DataMember(Name = "basicColliderColor")] public Color? BasicColliderColor { get; set; }
		[DataMember(Name = "triggerColliderColor")] public Color? TriggerColliderColor { get; set; }
		[DataMember(Name = "interiorColliderColor")] public Color? InteriorColliderColor { get; set; }
		[DataMember(Name = "theme")] public string Theme { get; set; }
	}
}
