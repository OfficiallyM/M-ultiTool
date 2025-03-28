﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Modules
{
	internal class Keybinds
	{
		private GUIStyle labelStyle = new GUIStyle();

		private int rebindAction = -1;
		private readonly Array keyCodes = Enum.GetValues(typeof(KeyCode));

		private Dictionary<string, Vector2> scrollPositions = new Dictionary<string, Vector2>();

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

		public List<Key> keys = new List<Key>();

		[DataContract]
		public class Key
		{
			[DataMember] public KeyCode key = KeyCode.None;
			[DataMember] public int action;
			[DataMember] public string name;
			[DataMember] public KeyCode defaultKey = KeyCode.None;

			public void Unset()
			{
				key = KeyCode.None;
			}

			public void Set(KeyCode _key)
			{
				Unset();
				key = _key;
			}

			public void Reset()
			{
				key = defaultKey;
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
					keys.Add(new Key() { action = i });
				}

				// Menu.
				keys[0].key = KeyCode.F1;
				keys[0].defaultKey = KeyCode.F1;
				keys[0].name = "Open menu";

				// Delete mode.
				keys[1].key = KeyCode.Delete;
				keys[1].defaultKey = KeyCode.Delete;
				keys[1].name = "Delete mode";

				// Noclip speed up.
				keys[2].key = KeyCode.LeftShift;
				keys[2].defaultKey = KeyCode.LeftShift;
				keys[2].name = "Noclip speed up";

				// Noclip fly up.
				keys[3].key = KeyCode.Space;
				keys[3].defaultKey = KeyCode.Space;
				keys[3].name = "Noclip fly up";

				// Noclip fly down.
				keys[4].key = KeyCode.LeftControl;
				keys[4].defaultKey = KeyCode.LeftControl;
				keys[4].name = "Noclip fly down";

				// Action 1.
				keys[5].key = KeyCode.Mouse0;
				keys[5].defaultKey = KeyCode.Mouse0;
				keys[5].name = "Action 1";

				// Action 2.
				keys[6].key = KeyCode.Mouse1;
				keys[6].defaultKey = KeyCode.Mouse1;
				keys[6].name = "Action 2";

				// Action 3.
				keys[7].key = KeyCode.E;
				keys[7].defaultKey = KeyCode.E;
				keys[7].name = "Action 3";

				// Action 4.
				keys[8].key = KeyCode.R;
				keys[8].defaultKey = KeyCode.R;
				keys[8].name = "Action 4";

				// Action 5.
				keys[9].key = KeyCode.F;
				keys[9].defaultKey = KeyCode.F;
				keys[9].name = "Action 5";

				// Up.
				keys[10].key = KeyCode.UpArrow;
				keys[10].defaultKey = KeyCode.UpArrow;
				keys[10].name = "Up";

				// Down.
				keys[11].key = KeyCode.DownArrow;
				keys[11].defaultKey = KeyCode.DownArrow;
				keys[11].name = "Down";

				// Left.
				keys[12].key = KeyCode.LeftArrow;
				keys[12].defaultKey = KeyCode.LeftArrow;
				keys[12].name = "Left";

				// Right.
				keys[13].key = KeyCode.RightArrow;
				keys[13].defaultKey = KeyCode.RightArrow;
				keys[13].name = "Right";

				// Select.
				keys[14].key = KeyCode.Return;
				keys[14].defaultKey = KeyCode.Return;
				keys[14].name = "Select";

                // Action 6.
                keys[15].key = KeyCode.V;
                keys[15].defaultKey = KeyCode.V;
                keys[15].name = "Action 6";
            }
			catch (Exception ex)
			{
				Logger.Log($"Keybind load error: {ex}", Logger.LogLevel.Error);
			}

			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;
		}

		public void OnLoad()
		{
			try
			{
				// Load the keybinds from the config.
				keys = MultiTool.Configuration.GetKeybinds(keys);
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
			return keys.Where(k => k.action == action).FirstOrDefault();
		}

		/// <summary>
		/// Get pretty name of action keybind.
		/// </summary>
		/// <param name="action">The action to get the key name of</param>
		/// <returns>Prettified key string</returns>
		public string GetPrettyName(int action)
		{
			KeyCode key = GetKeyByAction(action).key;

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
            Vector2 scrollPosition = GUILayout.BeginScrollView(scrollPositions.ContainsKey(title) ? scrollPositions[title] : new Vector2(0, 0));
            if (!scrollPositions.ContainsKey(title))
                scrollPositions.Add(title, scrollPosition);
            else
                scrollPositions[title] = scrollPosition;

			for (int i = 0; i < actions.Length; i++)
			{
				int action = actions[i];
				Key key = GetKeyByAction(action);

				GUILayout.Label($"{key.name} - Current ({key.key}) - Default ({keys[action].defaultKey})", labelStyle);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
				string rebindText = rebindAction == action ? "Waiting..." : "Rebind";
				if (GUILayout.Button(rebindText))
				{
					if (rebindAction == -1)
					{
						rebindAction = action;
					}
				}

                GUILayout.FlexibleSpace();

				if (GUILayout.Button("Reset"))
				{
					key.Reset();
                    MultiTool.Configuration.UpdateKeybinds(keys);
				}
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

			GUILayout.EndScrollView();

			if (rebindAction != -1)
			{
				Key key = GetKeyByAction(rebindAction);
				if (key != null && Input.anyKeyDown)
				{
					foreach (KeyCode keyCode in keyCodes)
					{
						if (Input.GetKey(keyCode) && keyCode != KeyCode.None)
						{
							key.Set(keyCode);
							rebindAction = -1;
							MultiTool.Configuration.UpdateKeybinds(keys);
						}
					}
				}
			}
            GUILayout.EndVertical();
            GUILayout.EndArea();
		}
	}
}
