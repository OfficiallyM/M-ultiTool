using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities.UI;
using MultiTool.Utilities;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class MaterialChanger : Core.VehicleConfigurationTab
	{
        public override string Name => "Material Changer";
		public override bool HasCache => true;

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
			if (mainscript.M.player == null || mainscript.M.player.Car == null) return;

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
					mainParent = SaveUtilities.SanitiseName(part.gameObject.name);

				string parent = part.transform.parent?.name ?? mainParent;
				parent = SaveUtilities.SanitiseName(parent);

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

				parentGroup.parts.Add(PartGroup.Create(SaveUtilities.SanitiseName(part.name), part, index, parentGroup.name));

				index++;
			}

			// Add any extra conditionless parts.
			MeshRenderer floor = GameUtilities.GetConditionlessVehiclePartByName(carObject, "Interior");
			if (floor != null)
				mainParentGroup.parts.Add(PartGroup.Create("Interior", floor, index + 1, mainParentGroup.name));
			MeshRenderer floor2 = GameUtilities.GetConditionlessVehiclePartByName(carObject, "Floor");
			if (floor2 != null)
				mainParentGroup.parts.Add(PartGroup.Create("Floor", floor2, index + 2, mainParentGroup.name));

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
			OnCacheRefresh();
		}

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			_car = mainscript.M.player.Car;
			tosaveitemscript save = _car.GetComponent<tosaveitemscript>();

			GUILayout.Label("Material changer", "LabelHeader");
			GUILayout.Label("Getting issues with materials not saving on a spawned vehicle?\nSpawn the vehicle, make a save, load that save then edit the materials.\nThe game has a weird issue where it doesn't name things properly.", "LabelSubHeader");
			GUILayout.Space(10);

			// Part selector.
			if (GUILayout.Button("Select parts", GUILayout.MaxWidth(400)))
				_partSelectorOpen = !_partSelectorOpen;

			if (_partSelectorOpen)
			{
				foreach (PartGroupParent parent in _materialParts)
				{
					GUILayout.Button($"Parent: {GetPrettyPartName(parent.name)}", "ButtonSecondaryTextLeft", GUILayout.MaxWidth(400));

					foreach (PartGroup group in parent.parts)
					{
						string partSelectText = $" ({group.index}) {GetPrettyPartName(group.name)}";
						if (GUILayout.Button(Accessibility.GetAccessibleString(partSelectText, IsGroupSelected(group)), "ButtonPrimaryTextLeft", GUILayout.MaxWidth(400)))
						{
							if (!IsGroupSelected(group))
							{
								Notifications.SendInformation("Material changer", $"Selected {GetPrettyPartName(group.name)}.", Notification.NotificationLength.VeryShort);
								_selectedParts.Add(group);
							}
							else
							{
								Notifications.SendInformation("Material changer", $"Deselected {GetPrettyPartName(group.name)}.", Notification.NotificationLength.VeryShort);
								DeselectByIndex(group.index);
							}
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
								GameUtilities.SetConditionlessPartMaterial(mesh, _selectedMaterial, materialColor);
								SaveUtilities.UpdateMaterials(new MaterialData()
								{
									ID = save.idInSave,
									part = selectedPart.name,
									parent = selectedPart.parent,
									isConditionless = true,
									exact = true,
									type = _selectedMaterial,
									color = materialColor
								});
							}
						}
						else
						{
							foreach (partconditionscript part in selectedPart.parts)
							{
								GameUtilities.SetPartMaterial(part, _selectedMaterial, materialColor);
								SaveUtilities.UpdateMaterials(new MaterialData()
								{
									ID = save.idInSave,
									part = selectedPart.name,
									parent = selectedPart.parent,
									exact = IsExact(selectedPart.name),
									type = _selectedMaterial,
									color = materialColor
								});
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
		/// Checks if a group is selected.
		/// </summary>
		/// <param name="group">Group to check</param>
		/// <returns>True if selected, otherwise false</returns>
		private bool IsGroupSelected(PartGroup group)
		{
			foreach (PartGroup selected in _selectedParts)
				if (selected.index == group.index) return true;

			return false;
		}

		/// <summary>
		/// Deselect a group by index.
		/// </summary>
		/// <param name="index">Group index to deselect</param>
		private void DeselectByIndex(int index)
		{
			foreach (PartGroup selected in _selectedParts)
			{
				if (selected.index == index)
				{
					_selectedParts.Remove(selected);
					return;
				}
			}
		}

		/// <summary>
		/// Make part name more user friendly.
		/// </summary>
		/// <param name="name">Part name to prettify</param>
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
