using MultiTool.Save;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace MultiTool.UI.Tabs
{
	internal class DebugTab : Tab
	{
		public override string Name => "Mod debug";
		public override bool ShowInNavigation => false;
		internal override bool IsFullScreen => true;
		private Vector2 _position;
		private string _data;

		public override void RenderTab(Rect dimensions)
		{
			if (string.IsNullOrEmpty(_data))
			{
				_data = JToken.Parse(SaveUtilities.GetRawSaveData()).ToString(Formatting.Indented);
			}

			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Back", GUILayout.MaxWidth(200)))
			{
				GUIRenderer.Tabs.ToggleActive(null);
			}
			GUILayout.Space(5);

			if (GUILayout.Button("Refresh", GUILayout.MaxWidth(200)))
			{
				_data = JToken.Parse(SaveUtilities.GetRawSaveData()).ToString(Formatting.Indented);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			_position = GUILayout.BeginScrollView(_position);
			_data = GUILayout.TextArea(_data);
			GUILayout.EndScrollView();

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
