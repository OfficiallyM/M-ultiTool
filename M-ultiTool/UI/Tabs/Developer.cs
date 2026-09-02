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

			// Toggle show object debug.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Object debug mode", Services.State.ObjectDebug), GUILayout.MaxWidth(200)))
			{
				Services.State.ObjectDebug = !Services.State.ObjectDebug;
			}

			if (Services.State.ObjectDebug)
			{
				// Toggle advanced object debug.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Enable advanced debug", Services.State.AdvancedObjectDebug), GUILayout.MaxWidth(200)))
				{
					Services.State.AdvancedObjectDebug = !Services.State.AdvancedObjectDebug;
				}
			}

			if (Services.State.AdvancedObjectDebug)
			{
				// Toggle showing Unity components.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Show Unity components", Services.State.ObjectDebugShowUnity), GUILayout.MaxWidth(200)))
				{
					Services.State.ObjectDebugShowUnity = !Services.State.ObjectDebugShowUnity;
				}

				// Toggle showing core components.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Show core game components", Services.State.ObjectDebugShowCore), GUILayout.MaxWidth(200)))
				{
					Services.State.ObjectDebugShowCore = !Services.State.ObjectDebugShowCore;
				}

				// Toggle showing child components.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Show child components", Services.State.ObjectDebugShowChildren), GUILayout.MaxWidth(200)))
				{
					Services.State.ObjectDebugShowChildren = !Services.State.ObjectDebugShowChildren;
				}
			}

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
