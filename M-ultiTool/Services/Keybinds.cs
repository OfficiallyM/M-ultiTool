using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace MultiTool.Services
{
	internal class Keybinds
	{
		private GUIStyle _labelStyle = new GUIStyle();

		private int _rebindAction = -1;
		private readonly Array _keyCodes = Enum.GetValues(typeof(KeyCode));

		private Dictionary<string, Vector2> _scrollPositions = new Dictionary<string, Vector2>();

		public enum Inputs
		{
			menu,
			deleteMode,
			noclipSpeedUp,
			noclipUp,
			noclipDown,
			action1,
			action2,
			action3,
			action4,
			action5,
			up,
			down,
			left,
			right,
			select,
			action6,
		}

		public List<Key> Keys = new List<Key>();

		[DataContract]
		public class Key
		{
			[DataMember(Name = "key")] public KeyCode AssignedKey = KeyCode.None;
			[DataMember(Name = "action")] public int Action;
			[DataMember(Name = "name")] public string Name;
			[DataMember(Name = "defaultKey")] public KeyCode DefaultKey = KeyCode.None;

			public void Unset()
			{
				AssignedKey = KeyCode.None;
			}

			public void Set(KeyCode _key)
			{
				Unset();
				AssignedKey = _key;
			}

			public void Reset()
			{
				AssignedKey = DefaultKey;
			}
		}

		public Keybinds()
		{
			try
			{
				// Load defaults.
				int maxInputs = (int)Enum.GetValues(typeof(Inputs)).Cast<Inputs>().Max();
				for (int i = 0; i <= maxInputs; i++)
				{
					Keys.Add(new Key() { Action = i });
				}

				// Menu.
				Keys[0].AssignedKey = KeyCode.F1;
				Keys[0].DefaultKey = KeyCode.F1;
				Keys[0].Name = "Open menu";

				// Delete mode.
				Keys[1].AssignedKey = KeyCode.Delete;
				Keys[1].DefaultKey = KeyCode.Delete;
				Keys[1].Name = "Delete mode";

				// Noclip speed up.
				Keys[2].AssignedKey = KeyCode.LeftShift;
				Keys[2].DefaultKey = KeyCode.LeftShift;
				Keys[2].Name = "Noclip speed up";

				// Noclip fly up.
				Keys[3].AssignedKey = KeyCode.Space;
				Keys[3].DefaultKey = KeyCode.Space;
				Keys[3].Name = "Noclip fly up";

				// Noclip fly down.
				Keys[4].AssignedKey = KeyCode.LeftControl;
				Keys[4].DefaultKey = KeyCode.LeftControl;
				Keys[4].Name = "Noclip fly down";

				// Action 1.
				Keys[5].AssignedKey = KeyCode.Mouse0;
				Keys[5].DefaultKey = KeyCode.Mouse0;
				Keys[5].Name = "Action 1";

				// Action 2.
				Keys[6].AssignedKey = KeyCode.Mouse1;
				Keys[6].DefaultKey = KeyCode.Mouse1;
				Keys[6].Name = "Action 2";

				// Action 3.
				Keys[7].AssignedKey = KeyCode.E;
				Keys[7].DefaultKey = KeyCode.E;
				Keys[7].Name = "Action 3";

				// Action 4.
				Keys[8].AssignedKey = KeyCode.R;
				Keys[8].DefaultKey = KeyCode.R;
				Keys[8].Name = "Action 4";

				// Action 5.
				Keys[9].AssignedKey = KeyCode.F;
				Keys[9].DefaultKey = KeyCode.F;
				Keys[9].Name = "Action 5";

				// Up.
				Keys[10].AssignedKey = KeyCode.UpArrow;
				Keys[10].DefaultKey = KeyCode.UpArrow;
				Keys[10].Name = "Up";

				// Down.
				Keys[11].AssignedKey = KeyCode.DownArrow;
				Keys[11].DefaultKey = KeyCode.DownArrow;
				Keys[11].Name = "Down";

				// Left.
				Keys[12].AssignedKey = KeyCode.LeftArrow;
				Keys[12].DefaultKey = KeyCode.LeftArrow;
				Keys[12].Name = "Left";

				// Right.
				Keys[13].AssignedKey = KeyCode.RightArrow;
				Keys[13].DefaultKey = KeyCode.RightArrow;
				Keys[13].Name = "Right";

				// Select.
				Keys[14].AssignedKey = KeyCode.Return;
				Keys[14].DefaultKey = KeyCode.Return;
				Keys[14].Name = "Select";

				// Action 6.
				Keys[15].AssignedKey = KeyCode.V;
				Keys[15].DefaultKey = KeyCode.V;
				Keys[15].Name = "Action 6";
			}
			catch (Exception ex)
			{
				Logger.Log($"Keybind load error: {ex}", Logger.LogLevel.Error);
			}

			_labelStyle.alignment = TextAnchor.MiddleCenter;
			_labelStyle.normal.textColor = Color.white;
		}

		public void OnLoad()
		{
			try
			{
				// Load the keybinds from the config.
				Keys = MultiTool.Configuration.GetKeybinds(Keys);
			}
			catch (Exception ex)
			{
				Logger.Log($"Keybinds load error - {ex}", Logger.LogLevel.Error);
			}
		}

		/// <summary>
		/// Find the key for a specified action
		/// </summary>
		/// <param name="action">The action to search by</param>
		/// <returns>The key</returns>
		public Key GetKeyByAction(int action)
		{
			return Keys.Where(k => k.Action == action).FirstOrDefault();
		}

		/// <summary>
		/// Get pretty name of action keybind.
		/// </summary>
		/// <param name="action">The action to get the key name of</param>
		/// <returns>Prettified key string</returns>
		public string GetPrettyName(int action)
		{
			KeyCode key = GetKeyByAction(action).AssignedKey;

			switch (key)
			{
				case KeyCode.Mouse0:
					return "Left mouse button";
				case KeyCode.Mouse1:
					return "Right mouse button";
				default:
					return key.ToString();
			}
		}

		/// <summary>
		/// <para>Render a rebind menu</para>
		/// <para>This should be called from an OnGUI function</para>
		/// </summary>
		/// <param name="title">The menu title</param>
		/// <param name="actions">Int array of actions to display rebinds for</param>
		/// <param name="x">The X position to display the menu</param>
		/// <param name="y">The Y position to display the menu</param>
		/// <param name="width">Width of the menu</param>
		/// <param name="height">Height of the menu</param>
		public void RenderRebindMenu(string title, int[] actions, float x, float y, float width, float height)
		{
			if (actions.Length == 0)
				return;

			GUILayout.BeginArea(new Rect(x, y, width, height), $"<size=16><b>{title}</b></size>", "box");
			GUILayout.BeginVertical();
			GUILayout.Space(30);
			Vector2 scrollPosition = GUILayout.BeginScrollView(_scrollPositions.ContainsKey(title) ? _scrollPositions[title] : new Vector2(0, 0));
			if (!_scrollPositions.ContainsKey(title))
				_scrollPositions.Add(title, scrollPosition);
			else
				_scrollPositions[title] = scrollPosition;

			for (int i = 0; i < actions.Length; i++)
			{
				int action = actions[i];
				Key key = GetKeyByAction(action);

				GUILayout.Label($"{key.Name} - Current ({key.AssignedKey}) - Default ({Keys[action].DefaultKey})", _labelStyle);

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				string rebindText = _rebindAction == action ? "Waiting..." : "Rebind";
				if (GUILayout.Button(rebindText))
				{
					if (_rebindAction == -1)
					{
						_rebindAction = action;
					}
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Reset"))
				{
					key.Reset();
					MultiTool.Configuration.UpdateKeybinds(Keys);
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.Space(10);
			}

			GUILayout.EndScrollView();

			if (_rebindAction != -1)
			{
				Key key = GetKeyByAction(_rebindAction);
				if (key != null && Input.anyKeyDown)
				{
					foreach (KeyCode keyCode in _keyCodes)
					{
						if (Input.GetKey(keyCode) && keyCode != KeyCode.None)
						{
							key.Set(keyCode);
							_rebindAction = -1;
							MultiTool.Configuration.UpdateKeybinds(Keys);
						}
					}
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
