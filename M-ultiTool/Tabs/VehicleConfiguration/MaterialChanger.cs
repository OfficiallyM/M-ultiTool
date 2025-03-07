using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities.UI;
using MultiTool.Utilities;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class MaterialChanger : Core.VehicleConfigurationTab
	{
        public override string Name => "MaterialChanger";

		private Vector2 _position;
		private carscript _car;

		private Dictionary<string, string> _materials = new Dictionary<string, string>()
		{
			{ "coloredleather", "Seat Leather" },
			{ "leather", "Sun visor leather" },
			{ "chleathercar08c", "Leather" },
			{ "fociw", "Leather 2" },
			{ "focib", "Leather 3" },
			{ "cleather", "Leather 4" },
			{ "huzat01", "Fabric 1" },
			{ "huzat02", "Fabric 2" },
			{ "huzat03", "Fabric 3" },
			{ "huzat04", "Fabric 4" },
			{ "karpit", "Cardboard" },
			{ "wood", "Wood" },
			{ "firearmwood", "Wood 2" },
			{ "busfa", "Wood 3" },
			{ "metals", "Metal" },
			{ "metals2", "Metal 2" },
			{ "buslepcso", "Metal 3" },
			{ "buspadlo", "Metal 4" },
			{ "darkmetal", "Dark metal" },
			{ "regilampaszin", "Lamp metal" },
			{ "gumi", "Tire rubber" },
			{ "fehergumi", "Tire rubber 2" },
			{ "nyulsz01", "Rabbit fur" },
			{ "szivacs2", "Sponge" },
			{ "tarbanckarpit", "Bakelite" },
			{ "csodapaint", "Metal painted" },
			{ "car08paint", "Metal painted 2" },
			{ "tarbancpaint", "Duroplast painted" },
			{ "busfem", "Plastic 1" },
			{ "busteto", "Plastic 2" },
			{ "metalrevolver", "Revolver" },
			{ "radiator", "Radiator" },
			{ "car07csik", "Fury stripe white" },
			{ "car07csik2", "Fury stripe gold" },
		};
		private List<PartGroup> _materialParts = new List<PartGroup>();
		private bool _partSelectorOpen = false;
		private bool _materialSelectorOpen = false;
		private PartGroup _selectedPart = null;
		private string _selectedMaterial = null;
		private bool _colorSelectorOpen = false;

		public override void OnCacheRefresh()
		{
			if (_car == null)
				_car = mainscript.M.player.Car;

			_materialParts.Clear();
			GameObject carObject = _car.gameObject;

			// Add all parts with a condition.
			foreach (partconditionscript part in _car.GetComponentsInChildren<partconditionscript>())
				_materialParts.Add(PartGroup.Create(part.name, part));

			// Add any extra conditionless parts.
			MeshRenderer floor = GameUtilities.GetConditionlessVehiclePartByName(carObject, "Interior");
			if (floor != null)
				_materialParts.Add(PartGroup.Create("Interior", floor));
			MeshRenderer floor2 = GameUtilities.GetConditionlessVehiclePartByName(carObject, "Floor");
			if (floor2 != null)
				_materialParts.Add(PartGroup.Create("Floor", floor2));
		}

		public override void OnVehicleChange()
		{
			_selectedPart = null;
		}

		public override void RenderTab(Rect dimensions)
		{
			dimensions.width /= 2;
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			_car = mainscript.M.player.Car;
			tosaveitemscript save = _car.GetComponent<tosaveitemscript>();

			GUILayout.Label("Material changer", "LabelHeader");

			// Part selector.
			string partSelectString = "Select part";
			if (_selectedPart != null)
				partSelectString = $"Part: {GetPrettyPartName(_selectedPart.name)}";
			if (GUILayout.Button(partSelectString, GUILayout.MaxWidth(400)))
				_partSelectorOpen = !_partSelectorOpen;

			if (_partSelectorOpen)
			{
				if (GUILayout.Button("None", GUILayout.MaxWidth(400)))
				{
					_selectedPart = null;
					_partSelectorOpen = false;
				}
				foreach (PartGroup group in _materialParts)
				{
					string parent = group.parts?[0]?.transform.parent?.name;
					// Hide parent if name matches part name.
					if (parent != null && GetPrettyPartName(parent) == GetPrettyPartName(group.name))
						parent = null;
					if (parent != null)
						parent = $"(Parent: {GetPrettyPartName(parent)})";
					if (GUILayout.Button($"{GetPrettyPartName(group.name)} {(parent != null ? parent : "")}", GUILayout.MaxWidth(400)))
					{
						_selectedPart = group;
						_partSelectorOpen = false;
					}
				}
			}

			if (_selectedPart != null)
			{
				// Material selector.
				string materialSelectString = "Select material";
				if (_selectedMaterial != null)
					materialSelectString = $"Material: {_materials[_selectedMaterial]}";
				if (GUILayout.Button(materialSelectString, GUILayout.MaxWidth(400)))
					_materialSelectorOpen = !_materialSelectorOpen;


				if (_materialSelectorOpen)
				{
					if (GUILayout.Button("None", GUILayout.MaxWidth(400)))
					{
						_selectedMaterial = null;
						_materialSelectorOpen = false;
					}
					foreach (KeyValuePair<string, string> material in _materials)
					{
						if (GUILayout.Button(material.Value, GUILayout.MaxWidth(400)))
						{
							_selectedMaterial = material.Key;
							_materialSelectorOpen = false;
						}
					}
				}

				Color? materialColor = null;

				// Colour selector.
				if (_selectedMaterial != null)
				{
					if (GUILayout.Button("Toggle color selector", GUILayout.MaxWidth(400)))
						_colorSelectorOpen = !_colorSelectorOpen;
				}

				if (_colorSelectorOpen)
					materialColor = Colour.RenderColourSliders(dimensions.width / 2, GUIRenderer.materialColor);

				if (_selectedMaterial != null && GUILayout.Button("Apply", GUILayout.MaxWidth(400)))
				{
					if (_selectedPart.IsConditionless())
					{
						foreach (MeshRenderer mesh in _selectedPart.meshes)
						{
							Thread thread = new Thread(() =>
							{
								GameUtilities.SetConditionlessPartMaterial(mesh, _selectedMaterial, materialColor);
								SaveUtilities.UpdateMaterials(new MaterialData()
								{
									ID = save.idInSave,
									part = _selectedPart.name,
									isConditionless = true,
									exact = IsExact(_selectedPart.name),
									type = _selectedMaterial,
									color = materialColor
								});
							});
							thread.Start();
						}
					}
					else
					{
						foreach (partconditionscript part in _selectedPart.parts)
						{
							Thread thread = new Thread(() =>
							{
								GameUtilities.SetPartMaterial(part, _selectedMaterial, materialColor);
								SaveUtilities.UpdateMaterials(new MaterialData()
								{
									ID = save.idInSave,
									part = _selectedPart.name,
									exact = IsExact(_selectedPart.name),
									type = _selectedMaterial,
									color = materialColor
								});
							});
							thread.Start();
						}
					}
				}
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		/// <summary>
		/// Make part name more user friendly.
		/// </summary>
		/// <param name="random">Part name to prettify</param>
		/// <returns>Prettified part name</returns>
		private string PrettifyName(string name)
		{
			return name.Replace("(Clone)", string.Empty);
		}

		/// <summary>
		/// Make part name more user friendly.
		/// </summary>
		/// <param name="part">Part name to translate</param>
		/// <returns>Prettified part name</returns>
		private string GetPrettyPartName(string part)
		{
			part = PrettifyName(part);

			switch (part)
			{
				case "PartConColorLeather":
					return "Main seats";
				case "NapellenzoLeft":
					return "Left sun visor";
				case "NapellenzoRight":
					return "Right sun visor";
				case "GloveStore":
					return "Glove box";
				case "Karpit":
					return "Headliner";
				case "PartConCsik":
					return "Fury stripe";
				case "PartConMetal":
					return "Metal";
				case "PartConMetals":
					return "Metals";
				case "PartConMetals2":
					return "Metals 2";
				case "PartConLeather":
					return "Leather";
				case "PartConDarkMetal":
					return "Dark metals";
				case "PartConConv":
					return "Soft top roof";
				case "PartConCar03Karpit":
					return "Beetle shelf";
				case "Interior":
				case "Floor":
					return "Carpet";
			}

			string partLower = part.ToLower();

			if (partLower.Contains("seat"))
				return "Removable seat";

			if (partLower.Contains("felni"))
				return "Wheel";

			if (partLower.Contains("gumi"))
				return "Tire";

			if (partLower.Contains("disztarcsa"))
				return "Hubcap";

			if (partLower.Contains("coolant"))
				return "Radiator";

			if (partLower.Contains("kesztyutarto"))
				return "Glove box";

			return part;
		}

		/// <summary>
		/// Whether the part matches by exact name.
		/// </summary>
		/// <param name="part">Part name to check</param>
		/// <returns>Returns true if the part matches off exact name, otherwise false.</returns>
		private bool IsExact(string part)
		{
			switch (part)
			{
				case "PartConColorLeather":
				case "NapellenzoLeft":
				case "NapellenzoRight":
					return true;
			}
			return false;
		}
	}
}
