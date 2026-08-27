using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static MultiTool.Services.Keybinds;

namespace MultiTool.Config
{
	[DataContract]
	internal class ConfigSerializable
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
