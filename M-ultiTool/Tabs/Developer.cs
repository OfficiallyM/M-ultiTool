using MultiTool.Core;
using UnityEngine;
using Settings = MultiTool.Core.Settings;
using MultiTool.Utilities.UI;

namespace MultiTool.Tabs
{
	internal class DeveloperTab : Tab
	{
		public override string Name => "Developer Tools";

		private Settings _settings = new Settings();
		private Vector2 _position;

		public override void RenderTab(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			// Toggle show coords.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Show coords", _settings.showCoords), GUILayout.MaxWidth(200)))
			{
				_settings.showCoords = !_settings.showCoords;
			}

			// Toggle show object debug.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Object debug mode", _settings.objectDebug), GUILayout.MaxWidth(200)))
			{
				_settings.objectDebug = !_settings.objectDebug;
			}

			if (_settings.objectDebug)
			{
				// Toggle advanced object debug.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Enable advanced debug", _settings.advancedObjectDebug), GUILayout.MaxWidth(200)))
				{
					_settings.advancedObjectDebug = !_settings.advancedObjectDebug;
				}
			}

			if (_settings.advancedObjectDebug)
			{
				// Toggle showing Unity components.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Show Unity components", _settings.objectDebugShowUnity), GUILayout.MaxWidth(200)))
				{
					_settings.objectDebugShowUnity = !_settings.objectDebugShowUnity;
				}

				// Toggle showing core components.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Show core game components", _settings.objectDebugShowCore), GUILayout.MaxWidth(200)))
				{
					_settings.objectDebugShowCore = !_settings.objectDebugShowCore;
				}

				// Toggle showing child components.
				if (GUILayout.Button(Accessibility.GetAccessibleString("Show child components", _settings.objectDebugShowChildren), GUILayout.MaxWidth(200)))
				{
					_settings.objectDebugShowChildren = !_settings.objectDebugShowChildren;
				}
			}

			// Toggle showing colliders.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Show colliders", _settings.showColliders), GUILayout.MaxWidth(200)))
			{
				_settings.showColliders = !_settings.showColliders;
			}

			// Toggle showing collider help.
			if (GUILayout.Button(Accessibility.GetAccessibleString("Show collider help", _settings.showColliderHelp), GUILayout.MaxWidth(200)))
			{
				_settings.showColliderHelp = !_settings.showColliderHelp;
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
