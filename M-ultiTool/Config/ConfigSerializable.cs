using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static MultiTool.Services.Keybinds;

namespace MultiTool.Config
{
	[DataContract]
	internal class ConfigSerializable
	{
        [DataMember] public string version { get; set; }
		[DataMember] public List<Key> keybinds { get; set; }
		[DataMember] public float scrollWidth { get; set; }
		[DataMember] public int accessibility { get; set; }
		[DataMember] public bool? accessibilityModeAffectsColor { get; set; }
		[DataMember] public float noclipFastMoveFactor { get; set; }
		[DataMember] public List<Color> palette { get; set; }
        [DataMember] public Color? basicColliderColor { get; set; }
        [DataMember] public Color? triggerColliderColor { get; set; }
        [DataMember] public Color? interiorColliderColor { get; set; }
		[DataMember] public string theme { get; set; }
    }
}
