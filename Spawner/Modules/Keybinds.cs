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

		public enum Inputs
		{
			menu,
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

				keys[0].key = KeyCode.F1;
				keys[0].defaultKey = KeyCode.F1;
				keys[0].name = "Open menu";				
			}
			catch (Exception ex)
			{
				logger.Log($"Keybind load error: {ex}", Logger.LogLevel.Error);
			}

			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;
		}

		public void OnLoad()
		{
			try
			{
				// Load the keybinds from the config.
				List<Key> newKeys = config.GetKeybinds();
				if (newKeys == null)
					// No keybinds in config, write the defaults.
					config.UpdateKeybinds(keys);
				else
					keys = newKeys;
			}
			catch (Exception ex)
			{
				logger.Log($"Keybinds Onload {ex}", Logger.LogLevel.Error);
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
		/// <para>Render a rebind menu</para>
		/// <para>This should be called from an OnGUI function</para>
		/// </summary>
		/// <param name="title">The menu title</param>
		/// <param name="actions">Int array of actions to display rebinds for</param>
		/// <param name="x">The X position to display the menu</param>
		/// <param name="y">The Y position to display the menu</param>
		public void RenderRebindMenu(string title, int[] actions, float x, float y)
		{
			if (actions.Length == 0)
				return;

			float width = 300f;
			float actionHeight = 40f;
			float actionY = y + 25f;
			float labelWidth = 295f;
			float buttonWidth = 80f;
			float height = 30f + (actions.Length * actionHeight);
			GUI.Box(new Rect(x, y, width, height), $"<size=16><b>{title}</b></size>");

			for (int i = 0; i < actions.Length; i++)
			{
				int action = actions[i];
				Key key = GetKeyByAction(action);

				GUI.Label(new Rect(x + 10f, actionY, labelWidth, actionHeight / 2), $"{key.name} - Current ({key.key}) - Default ({key.defaultKey})", labelStyle);
				actionY += actionHeight / 2;

				float buttonX = x + width / 4.5f;

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
