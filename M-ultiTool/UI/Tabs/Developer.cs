using MultiTool.Database;
using UnityEngine;

namespace MultiTool.UI.Tabs
{
	internal class DeveloperTab : Tab
	{
		public override string Name => "Developer Tools";

		private Vector2 _position;

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			MultiTool.Tools.RenderControl("show_coords");
			MultiTool.Tools.RenderControl("object_debug");

			// Toggle showing colliders.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Show colliders", Services.State.ShowColliders), GUILayout.MaxWidth(200)))
			{
				Services.State.ShowColliders = !Services.State.ShowColliders;
			}

			// Toggle showing collider help.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Show collider help", Services.State.ShowColliderHelp), GUILayout.MaxWidth(200)))
			{
				Services.State.ShowColliderHelp = !Services.State.ShowColliderHelp;
			}

			if (GUILayout.Button("Rebuild thumbnail cache (this will lag)", "ButtonPrimaryWrap", GUILayout.MaxWidth(200)))
				ThumbnailGenerator.RebuildCache();

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
