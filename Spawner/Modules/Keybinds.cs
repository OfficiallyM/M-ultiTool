using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpawnerTLD.Modules
{
	internal class Keybinds
	{
		// Modules.
		private Logger logger;
		private Config config;

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
			copy,
			paste
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

		public Keybinds(Logger _logger, Config _config)
		{
			logger = _logger;
			config = _config;

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

				// Copy.
				keys[5].key = KeyCode.Mouse0;
				keys[5].defaultKey = KeyCode.Mouse0;
				keys[5].name = "Copy";

				// Paste.
				keys[6].key = KeyCode.Mouse1;
				keys[6].defaultKey = KeyCode.Mouse1;
				keys[6].name = "Paste";
			}
			catch (Exception ex)
			{
				logger.Log($"Keybind load error: {ex}", Logger.LogLevel.Error);
			}

			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.normal.textColor = Color.white;
		}

		public void OnLoad()
		{
			try
			{
				// Load the keybinds from the config.
				keys = config.GetKeybinds(keys);
			}
			catch (Exception ex)
			{
				logger.Log($"Keybinds load error - {ex}", Logger.LogLevel.Error);
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
		public void RenderRebindMenu(string title, int[] actions, float x, float y, float? widthOverride = null, float? heightOverride = null)
		{
			if (actions.Length == 0)
				return;

			float width = 300f;
			float actionHeight = 40f;
			float actionY = y + 25f;
			float labelWidth = 295f;
			float buttonWidth = 80f;
			float height = 30f + (actions.Length * (actionHeight + 5f));

			if (widthOverride != null)
				width = widthOverride.Value;

			if (heightOverride != null)
				height = heightOverride.Value;

			float scrollHeight = height - 35f;
			if (height > Screen.height * 0.9f)
				height = Screen.height * 0.9f;

			GUI.Box(new Rect(x, y, width, height), $"<size=16><b>{title}</b></size>");

			Vector2 scrollPosition = GUI.BeginScrollView(new Rect(x + 10f, y + 25f, width - 20f, height - 35f), scrollPositions.ContainsKey(title) ? scrollPositions[title] : new Vector2(0, 0), new Rect(x + 10f, y + 25f, width - 20f, scrollHeight), new GUIStyle(), new GUIStyle());
			if (!scrollPositions.ContainsKey(title))
				scrollPositions.Add(title, scrollPosition);
			else
				scrollPositions[title] = scrollPosition;

			for (int i = 0; i < actions.Length; i++)
			{
				int action = actions[i];
				Key key = GetKeyByAction(action);

				GUI.Label(new Rect(x + 10f, actionY, labelWidth, actionHeight / 2), $"{key.name} - Current ({key.key}) - Default ({key.defaultKey})", labelStyle);
				actionY += actionHeight / 2;

				float buttonX = x + 10f;

				string rebindText = rebindAction == action ? "Waiting..." : "Rebind";
				if (GUI.Button(new Rect(buttonX, actionY, buttonWidth, actionHeight / 2), rebindText))
				{
					if (rebindAction == -1)
					{
						rebindAction = action;
					}
				}

				buttonX += buttonWidth + 10f;

				if (GUI.Button(new Rect(buttonX, actionY, buttonWidth, actionHeight / 2), "Reset"))
				{
					key.Reset();
					config.UpdateKeybinds(keys);
				}

				actionY += actionHeight + 5f;
			}

			GUI.EndScrollView();

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
							config.UpdateKeybinds(keys);
						}
					}
				}
			}
		}
	}
}
