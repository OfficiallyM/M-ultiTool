using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MultiTool.Modules;
using Logger = MultiTool.Modules.Logger;
using TLDLoader;
using UnityEngine.UI;
using Settings = MultiTool.Core.Settings;

namespace MultiTool.Tabs
{
	internal class DeveloperTab : Tab
	{
		public override string Name => "Developer Tools";

		private Settings settings = new Settings();
		private Vector2 currentPosition;
		public override void RenderTab(Dimensions dimensions)
		{
			float x = dimensions.x + 10f;
			float y = dimensions.y + 10f;
			float buttonWidth = 200f;
			float buttonHeight = 20f;
			float sliderWidth = 300f;
			float headerWidth = dimensions.width - 20f;
			float headerHeight = 40f;

			float scrollHeight = 100f;

			currentPosition = GUI.BeginScrollView(new Rect(x, y, dimensions.width - 20f, dimensions.height - 20f), currentPosition, new Rect(x, y, dimensions.width - 20f, scrollHeight), new GUIStyle(), GUI.skin.verticalScrollbar);

			// Toggle show coords.
			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Show coords", settings.showCoords)))
			{
				settings.showCoords = !settings.showCoords;
			}
			y += buttonHeight + 10f;

			GUI.EndScrollView();
		}
	}
}
