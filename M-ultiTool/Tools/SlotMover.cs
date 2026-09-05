using MultiTool.Extensions;
using MultiTool.Save;
using MultiTool.Services;
using MultiTool.UI;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Tools
{
	internal class SlotMoverTool : Tool
	{
		public override string Name => "Slot Mover";

		private carscript _car;
		private tosaveitemscript _carSave;
		private string _slotStage = "slotSelect";
		private GameObject _selectedSlot;
		private GameObject _hoveredSlot;
		private int _hoveredSlotIndex = 0;
		private int _previousHoveredSlotIndex = 0;
		private bool _slotMoverFirstRun = true;
		private Vector3 _selectedSlotResetPosition;
		private Quaternion _selectedSlotResetRotation;
		private float[] _moveOptions = new float[] { 10f, 1f, 0.1f, 0.01f, 0.001f };
		private float _moveValue = 0.1f;
		private List<GameObject> _slots = new List<GameObject>();

		public override void ControlRender()
		{
			string name = Name.ToLowerInvariant();
			if (GUILayout.Button(Accessibility.GetAccessibleString($"Toggle {name}", Tools.IsActive(Id)), GUILayout.MaxWidth(200)))
				Tools.Toggle(Id);
			GUILayout.Space(10);
		}

		public override void OnActivate()
		{
			CacheVehicleData();
		}

		public override void OnDeactivate()
		{
			SlotMoverDispose();
		}

		public override void Update()
		{
			try
			{ 
				switch (_slotStage)
				{
					case "slotSelect":
						bool slotChanged = false;

						// Render collider on first load.
						if (_slotMoverFirstRun)
						{
							slotChanged = true;
							_hoveredSlot = _slots[_hoveredSlotIndex];
						}

						// Move selector left.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.left).AssignedKey))
						{
							_previousHoveredSlotIndex = _hoveredSlotIndex;
							_hoveredSlotIndex--;
							if (_hoveredSlotIndex < 0)
								_hoveredSlotIndex = _slots.Count - 1;

							_hoveredSlot = _slots[_hoveredSlotIndex];
							slotChanged = true;
						}

						// Move selector right.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.right).AssignedKey))
						{
							_previousHoveredSlotIndex = _hoveredSlotIndex;
							_hoveredSlotIndex++;
							if (_hoveredSlotIndex >= _slots.Count)
								_hoveredSlotIndex = 0;

							_hoveredSlot = _slots[_hoveredSlotIndex];
							slotChanged = true;
						}

						// Select the hovered slot.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
						{
							_slotStage = "move";
							_selectedSlot = _hoveredSlot;

							_selectedSlotResetPosition = _selectedSlot.transform.localPosition;
							_selectedSlotResetRotation = _selectedSlot.transform.localRotation;

							// Get reset positions from save data.
							SlotData slotData = SaveUtilities.GetSlotData(_carSave.idInSave, _selectedSlot.name);
							if (slotData != null)
							{
								_selectedSlotResetPosition = slotData.ResetPosition;
								_selectedSlotResetRotation = slotData.ResetRotation;
							}

							SlotMoverSelectDispose();

							ObjectUtilities.ShowColliders(_selectedSlot, Color.blue);
						}

						if (slotChanged)
						{
							ObjectUtilities.ShowColliders(_hoveredSlot, Color.red);

							if (!_slotMoverFirstRun)
							{
								GameObject previousSlot = _slots[_previousHoveredSlotIndex];

								ObjectUtilities.DestroyColliders(previousSlot);
							}
							_slotMoverFirstRun = false;
						}
						break;
					case "move":
						// Deselect slot.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
						{
							_slotStage = "slotSelect";
							_hoveredSlotIndex = Array.FindIndex(_slots.ToArray(), s => s.name == _selectedSlot.name);
							SlotMoverMoveDispose();
							return;
						}

						// Switch to rotate mode.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action3).AssignedKey))
						{
							_slotStage = "rotate";
						}

						// Change move amount.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action5).AssignedKey))
						{
							int currentIndex = Array.FindIndex(_moveOptions, s => s == _moveValue);
							if (currentIndex == -1 || currentIndex == _moveOptions.Length - 1)
								_moveValue = _moveOptions[0];
							else
								_moveValue = _moveOptions[currentIndex + 1];
						}

						Transform partTransform = _selectedSlot.transform;
						Vector3 oldPos = partTransform.localPosition;

						// Move forward.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey))
						{
							partTransform.localPosition += Vector3.forward * _moveValue;
						}

						// Move backwards.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey))
						{
							partTransform.localPosition += Vector3.back * _moveValue;
						}

						// Move left.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.left).AssignedKey))
						{
							partTransform.localPosition += Vector3.left * _moveValue;
						}

						// Move right.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.right).AssignedKey))
						{
							partTransform.localPosition += Vector3.right * _moveValue;
						}

						// Move up.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).AssignedKey))
						{
							partTransform.localPosition += Vector3.up * _moveValue;
						}

						// Move down.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).AssignedKey))
						{
							partTransform.localPosition += Vector3.down * _moveValue;
						}

						// Reset position.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action4).AssignedKey))
						{
							partTransform.localPosition = _selectedSlotResetPosition;
						}

						// Check if position has changed.
						if (oldPos != partTransform.localPosition)
						{
							SlotData slotData = new SlotData()
							{
								ID = _carSave.idInSave,
								Slot = _selectedSlot.name,
								Position = partTransform.localPosition,
								ResetPosition = _selectedSlotResetPosition,
								Rotation = partTransform.localRotation,
								ResetRotation = _selectedSlotResetRotation,
							};
							SaveUtilities.UpdateSlot(slotData);
						}

						break;
					case "rotate":
						// Deselect slot.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.select).AssignedKey))
						{
							_slotStage = "slotSelect";
							_hoveredSlotIndex = Array.FindIndex(_slots.ToArray(), s => s.name == _selectedSlot.name);
							SlotMoverMoveDispose();
							return;
						}

						// Switch to move mode.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action3).AssignedKey))
						{
							_slotStage = "move";
						}

						// Change move amount.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action5).AssignedKey))
						{
							int currentIndex = Array.FindIndex(_moveOptions, s => s == _moveValue);
							if (currentIndex == -1 || currentIndex == _moveOptions.Length - 1)
								_moveValue = _moveOptions[0];
							else
								_moveValue = _moveOptions[currentIndex + 1];
						}

						Transform rotatePartTransform = _selectedSlot.transform;
						Quaternion oldRot = rotatePartTransform.localRotation;

						// Rotate forward.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.up).AssignedKey))
						{
							rotatePartTransform.Rotate(Vector3.right, _moveValue);
						}

						// Rotate backwards.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.down).AssignedKey))
						{
							rotatePartTransform.Rotate(-Vector3.right, _moveValue);
						}

						// Rotate left.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.left).AssignedKey))
						{
							rotatePartTransform.Rotate(-Vector3.forward, _moveValue);
						}

						// Rotate right.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.right).AssignedKey))
						{
							rotatePartTransform.Rotate(Vector3.forward, _moveValue);
						}

						// Rotate anticlockwise.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.noclipSpeedUp).AssignedKey))
						{
							rotatePartTransform.Rotate(Vector3.up, _moveValue);
						}

						// Rotate clockwise.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.noclipDown).AssignedKey))
						{
							rotatePartTransform.Rotate(-Vector3.up, _moveValue);
						}

						// Reset position.
						if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action4).AssignedKey))
						{
							rotatePartTransform.localRotation = _selectedSlotResetRotation;
						}

						// Check if rotation has changed.
						if (oldRot != rotatePartTransform.localRotation)
						{
							SlotData slotData = new SlotData()
							{
								ID = _carSave.idInSave,
								Slot = _selectedSlot.name,
								Position = rotatePartTransform.localPosition,
								ResetPosition = _selectedSlotResetPosition,
								Rotation = rotatePartTransform.localRotation,
								ResetRotation = _selectedSlotResetRotation,
							};
							SaveUtilities.UpdateSlot(slotData);
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.Log($"Error during slotControl. Details: {ex}");
			}
		}

		public override void HudRender()
		{
			float fullWidth = Screen.width * 0.25f;
			float halfWidth = fullWidth / 2;

			switch (_slotStage)
			{
				case "slotSelect":
					int displayedSlots = 7;
					// Slots + left and right navigation hint buttons.
					int totalButtons = displayedSlots + 2;
					float width = Screen.width / totalButtons;

					// Possibly over-complicated method to show selected slot in the middle.
					int lowerHalf = Mathf.FloorToInt((displayedSlots - 1) / 2);
					int upperHalf = Mathf.CeilToInt(displayedSlots / 2) + 1;

					List<int> displayedIndexes = new List<int>();
					int countFrom = _hoveredSlotIndex - lowerHalf - 1;
					if (countFrom < 0)
						countFrom = _slots.Count - 1 - displayedSlots + upperHalf + _hoveredSlotIndex;
					else if (countFrom > _slots.Count - 1)
						countFrom = 0;
					for (int i = 1; i <= displayedSlots; i++)
					{
						int nextIndex;

						if (i <= lowerHalf || i >= upperHalf)
						{
							nextIndex = countFrom + 1;
							if (nextIndex > _slots.Count - 1)
							{
								nextIndex = 0;
								countFrom = 0;
							}
							else
							{
								countFrom = nextIndex;
							}

							displayedIndexes.Add(nextIndex);
						}
						else
						{
							displayedIndexes.Add(_hoveredSlotIndex);
							countFrom = _hoveredSlotIndex;
						}
					}

					GUILayout.BeginVertical();
					GUILayout.FlexibleSpace();

					GUILayout.BeginHorizontal();
					GUILayout.Button($"< ({Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.left)})", "ButtonSecondary", GUILayout.Width(width), GUILayout.Height(50));
					for (int index = 0; index < displayedIndexes.Count; index++)
					{
						int slotIndex = displayedIndexes[index];
						GameObject slot = _slots[slotIndex];
						string name = $"{slotIndex + 1} - {PrettifySlotName(slot.name)}";

						if (slotIndex == _hoveredSlotIndex)
						{
							GUILayout.Button($"<b>{name}</b>\nSelect ({Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.select)})", GUILayout.Width(width), GUILayout.Height(50));
						}
						else
						{
							GUILayout.Button(name, "ButtonSecondary", GUILayout.Width(width), GUILayout.Height(50));
						}
					}
					GUILayout.Button($"({Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.right)}) >", "ButtonSecondary", GUILayout.Width(width), GUILayout.Height(50));
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					break;

				case "move":
					GUILayout.BeginVertical();
					GUILayout.FlexibleSpace();

					GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
					GUILayout.Button($"Moving: {PrettifySlotName(_selectedSlot.name)}");

					GUILayout.BeginHorizontal();
					GUILayout.Button("Back to slot select", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.select), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Switch to rotate", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action3), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button($"Move by: {_moveValue}", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action5), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Up", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.noclipSpeedUp), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Down", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.noclipDown), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Left", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.left), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Right", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.right), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Forward", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.up), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Back", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.down), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Reset", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action4), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();

					GUILayout.FlexibleSpace();
					GUILayout.EndVertical();
					break;

				case "rotate":
					GUILayout.BeginVertical();
					GUILayout.FlexibleSpace();

					GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
					GUILayout.Button($"Rotating: {PrettifySlotName(_selectedSlot.name)}");

					GUILayout.BeginHorizontal();
					GUILayout.Button("Back to slot select", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.select), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Switch to move", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action3), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button($"Rotate by: {_moveValue}", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action5), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Clockwise", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.noclipDown), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Anticlockwise", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.noclipSpeedUp), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Left", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.left), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Right", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.right), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Forward", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.up), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Back", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.down), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Button("Reset", GUILayout.Width(halfWidth));
					GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action4), GUILayout.Width(halfWidth));
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();

					GUILayout.FlexibleSpace();
					GUILayout.EndVertical();
					break;
			}
		}

		private void CacheVehicleData()
		{
			_car = mainscript.M.player.Car;
			if (_car == null)
			{
				Notifications.SendWarning("Slot mover", "Sit in a vehicle to enable the slot mover.");
				return;
			}

			_carSave = _car.GetComponent<tosaveitemscript>();
			_slots.Clear();

			partslotscript[] partSlots = _car.GetComponentsInChildren<partslotscript>();
			foreach (partslotscript slot in partSlots)
			{
				GameObject obj = slot.gameObject;

				// Required as some slots don't have the actual part as a
				// child of the slot. These parts instead use a collider
				// which will either contain Col or Collider, so look for
				// either and use the parent instead.
				if (slot.name.Contains("Col"))
				{
					obj = slot.transform.parent.gameObject;
				}

				_slots.Add(obj);
			}

			// Find anything that isn't an actual part.
			foreach (MeshRenderer child in _car.GetComponentsInChildren<MeshRenderer>())
			{
				string name = PrettifySlotName(child.name).ToLower();
				GameObject parent = child.transform.parent.gameObject;
				string parentName = PrettifySlotName(parent.name).ToLower();
				string[] mufflers = new string[]
				{
					"muffler",
					"exhaust",
				};

				string[] parentNames = new string[]
				{
					"interiorlight",
					"plate",
				};

				foreach (string muffler in mufflers)
				{
					if ((name.Contains(muffler) || parentName.Contains(muffler)) && child.gameObject.activeSelf)
					{
						_slots.Add(child.gameObject);
					}
				}

				foreach (string parentSlotName in parentNames)
				{
					if (parentName.Contains(parentSlotName) && parent.activeSelf)
					{
						_slots.Add(parent);
					}
				}
			}

			// Add seat positions.
			foreach (seatscript seat in _car.GetComponentsInChildren<seatscript>())
			{
				if (seat.GetComponent<BoxCollider>() == null || seat.name.ToLower().Contains("col")) continue;
				_slots.Add(seat.gameObject);
			}
		}

		/// <summary>
		/// Make the vehicle slot name look prettier.
		/// </summary>
		/// <param name="name">Slot name</param>
		/// <returns>Prettified slot name</returns>
		private string PrettifySlotName(string name)
		{
			name = name.Replace("(Clone)", "");
			name = Regex.Replace(name, "\\((.*?)\\)", "");
			name = name.Trim();
			return name.IsAllLower() ? name.ToSentenceCase() : name;
		}

		/// <summary>
		/// Dispose of anything pertaining to slot mover.
		/// </summary>
		private void SlotMoverDispose()
		{
			_car = null;
			_carSave = null;
			_slotStage = "slotSelect";
			_slots.Clear();
			SlotMoverSelectDispose();
			SlotMoverMoveDispose();
		}

		/// <summary>
		/// Dispose of slot mover select stage.
		/// </summary>
		private void SlotMoverSelectDispose()
		{
			try
			{
				if (_hoveredSlot != null)
					ObjectUtilities.DestroyColliders(_hoveredSlot);

				_hoveredSlot = null;
				_hoveredSlotIndex = 0;
				_previousHoveredSlotIndex = 0;
				_slotMoverFirstRun = true;
			}
			catch (Exception ex)
			{
				Logger.Log($"Error occurred during slot mover select stage dispose. Details: {ex}", Logger.LogLevel.Warning);
			}
		}

		/// <summary>
		/// Dispose of slot mover move stage.
		/// </summary>
		private void SlotMoverMoveDispose()
		{
			try
			{
				if (_selectedSlot != null)
					ObjectUtilities.DestroyColliders(_selectedSlot);

				_selectedSlot = null;
				_selectedSlotResetPosition = Vector3.zero;
				_selectedSlotResetRotation.Set(0, 0, 0, 0);
			}
			catch (Exception ex)
			{
				Logger.Log($"Error occurred during slot mover move stage dispose. Details: {ex}", Logger.LogLevel.Warning);
			}
		}
	}
}
