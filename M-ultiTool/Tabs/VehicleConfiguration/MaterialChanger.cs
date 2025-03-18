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
        public override string Name => "Material Changer";

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
		private List<PartGroupParent> _materialParts = new List<PartGroupParent>();
		private bool _partSelectorOpen = false;
		private bool _materialSelectorOpen = false;
		private List<PartGroup> _selectedParts = new List<PartGroup>();
		private string _selectedMaterial = null;
		private bool _colorSelectorOpen = false;

		public override void OnCacheRefresh()
		{
			if (_car == null)
				_car = mainscript.M.player.Car;

			_materialParts.Clear();
			GameObject carObject = _car.gameObject;

			string mainParent = null;
			PartGroupParent mainParentGroup = null;

			// Add all parts with a condition.
			int index = 1;
			foreach (partconditionscript part in _car.GetComponentsInChildren<partconditionscript>())
			{
				if (mainParent == null)
					mainParent = part.gameObject.name;

				string parent = part.transform.parent?.name ?? mainParent;

				PartGroupParent parentGroup = null;
				foreach (PartGroupParent partParent in _materialParts)
				{
                    if (partParent.name == parent)
                    {
						parentGroup = partParent;
						break;
                    }
                }

				if (parentGroup == null) 
				{
					_materialParts.Add(PartGroupParent.Create(parent));
					parentGroup = _materialParts[_materialParts.Count - 1];
					if (parent == mainParent)
						mainParentGroup = parentGroup;
				}

				parentGroup.parts.Add(PartGroup.Create(part.name, part, index));

				index++;
			}

			// Add any extra conditionless parts.
			MeshRenderer floor = GameUtilities.GetConditionlessVehiclePartByName(carObject, "Interior");
			if (floor != null)
				mainParentGroup.parts.Add(PartGroup.Create("Interior", floor, index + 1));
			MeshRenderer floor2 = GameUtilities.GetConditionlessVehiclePartByName(carObject, "Floor");
			if (floor2 != null)
				mainParentGroup.parts.Add(PartGroup.Create("Floor", floor2, index + 2));

			// Reindex by group.
			index = 1;
			foreach (PartGroupParent parent in _materialParts)
			{
				foreach (PartGroup group in parent.parts)
				{
					group.index = index;
					index++;
				}
			}
		}

		public override void OnVehicleChange()
		{
			_selectedParts.Clear();
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
			if (GUILayout.Button("Select parts", GUILayout.MaxWidth(400)))
				_partSelectorOpen = !_partSelectorOpen;

			if (_partSelectorOpen)
			{
				foreach (PartGroupParent parent in _materialParts)
				{
					GUILayout.Button($"Parent: {GetPrettyPartName(parent.name)}", "ButtonPrimaryTextLeft", GUILayout.MaxWidth(400));

					foreach (PartGroup group in parent.parts)
					{
						if (GUILayout.Button($" ({group.index}) {GetPrettyPartName(group.name)}", "ButtonPrimaryTextLeft", GUILayout.MaxWidth(400)))
						{
							Notifications.SendInformation("Material changer", $"Selected {GetPrettyPartName(group.name)}.", Notification.NotificationLength.VeryShort);
							_selectedParts.Add(group);
						}
					}

					GUILayout.Space(2);
				}
			}

			GUILayout.Space(10);

			if (_selectedParts.Count > 0)
			{
				// Selected parts list.
				GUILayout.Label("Selected parts", "LabelSubHeader");
				foreach (PartGroup selectedPart in _selectedParts)
				{
					if (GUILayout.Button($" ({selectedPart.index}) {GetPrettyPartName(selectedPart.name)}", GUILayout.MaxWidth(400)))
					{
						_selectedParts.Remove(selectedPart);
						break;
					}
				}

				GUILayout.Space(10);

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

				GUILayout.Space(10);

				Color? materialColor = null;

				// Colour selector.
				if (_selectedMaterial != null)
				{
					if (GUILayout.Button("Toggle color selector", GUILayout.MaxWidth(400)))
						_colorSelectorOpen = !_colorSelectorOpen;
				}

				if (_colorSelectorOpen)
					materialColor = Colour.RenderColourSliders(dimensions.width / 2);

				if (_selectedMaterial != null && GUILayout.Button("Apply", GUILayout.MaxWidth(400)))
				{
					foreach (PartGroup selectedPart in _selectedParts)
					{
						if (selectedPart.IsConditionless())
						{
							foreach (MeshRenderer mesh in selectedPart.meshes)
							{
								Thread thread = new Thread(() =>
								{
									GameUtilities.SetConditionlessPartMaterial(mesh, _selectedMaterial, materialColor);
									SaveUtilities.UpdateMaterials(new MaterialData()
									{
										ID = save.idInSave,
										part = selectedPart.name,
										isConditionless = true,
										exact = IsExact(selectedPart.name),
										type = _selectedMaterial,
										color = materialColor
									});
								});
								thread.Start();
							}
						}
						else
						{
							foreach (partconditionscript part in selectedPart.parts)
							{
								Thread thread = new Thread(() =>
								{
									GameUtilities.SetPartMaterial(part, _selectedMaterial, materialColor);
									SaveUtilities.UpdateMaterials(new MaterialData()
									{
										ID = save.idInSave,
										part = selectedPart.name,
										exact = IsExact(selectedPart.name),
										type = _selectedMaterial,
										color = materialColor
									});
								});
								thread.Start();
							}
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
