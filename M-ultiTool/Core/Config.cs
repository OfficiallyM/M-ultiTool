﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MultiTool.Modules.Keybinds;

namespace MultiTool.Core
{
	// Serializable vehicle wrapper for translation config.
	[DataContract]
	internal class ConfigVehicle
	{
		[DataMember] public string objectName { get; set; }
		[DataMember] public int? variant { get; set; }
		[DataMember] public string name { get; set; }
	}

	[DataContract]
	internal class ConfigPOI
	{
		[DataMember] public string objectName { get; set; }
		[DataMember] public string name { get; set; }
	}

	[DataContract]
	internal class ConfigWrapper
	{
		[DataMember] public List<ConfigVehicle> vehicles { get; set; }
		[DataMember] public List<ConfigPOI> POIs { get; set; }
	}

	internal class Settings
	{
		public static bool s_deleteMode = false;
		public bool deleteMode
		{
			get { return s_deleteMode; }
			set { s_deleteMode = value; }
		}

		public static bool s_godMode = false;
		public bool godMode
		{
			get { return s_godMode; }
			set { s_godMode = value; }
		}

		public static bool s_noclip = false;
		public bool noclip
		{
			get { return s_noclip; }
			set { s_noclip = value; }
		}

		public static bool s_spawnWithFuel = true;
		public bool spawnWithFuel
		{
			get { return s_spawnWithFuel; }
			set { s_spawnWithFuel = value; }
		}

		public static string s_mode = null;
		public string mode
		{
			get { return s_mode; }
			set { s_mode = value; }
		}

		public static bool s_showCoords = false;
		public bool showCoords
		{
			get { return s_showCoords; }
			set { s_showCoords = value; }
		}

		public static bool s_objectDebug = false;
		public bool objectDebug
		{
			get { return s_objectDebug; }
			set { s_objectDebug = value; }
		}

		public static bool s_advancedObjectDebug = false;
		public bool advancedObjectDebug
        {
			get { return s_advancedObjectDebug; }
			set { s_advancedObjectDebug = value; }
		}

		public static bool s_objectDebugShowUnity = true;
		public bool objectDebugShowUnity
		{
			get { return s_objectDebugShowUnity; }
			set { s_objectDebugShowUnity = value; }
		}

		public static bool s_objectDebugShowCore = true;
		public bool objectDebugShowCore
		{
			get { return s_objectDebugShowCore; }
			set { s_objectDebugShowCore = value; }
		}
	}

	[DataContract]
	internal class PlayerData
	{
		[DataMember] public float walkSpeed { get; set; }
		[DataMember] public float runSpeed { get; set; }
		[DataMember] public float jumpForce { get; set; }
		[DataMember] public float pushForce { get; set; }
		[DataMember] public float carryWeight { get; set; }
	}

	[DataContract]
	internal class ConfigSerializable
	{
		[DataMember] public List<Key> keybinds { get; set; }
		[DataMember] public bool? legacyUI { get; set; }
		[DataMember] public float scrollWidth { get; set; }
		[DataMember] public bool? noclipGodmodeDisable { get; set; }
		[DataMember] public string accessibilityMode { get; set; }
		[DataMember] public float noclipFastMoveFactor { get; set; }
		[DataMember] public List<Color> palette { get; set; }
		[DataMember] public PlayerData playerData { get; set; }
	}
}
