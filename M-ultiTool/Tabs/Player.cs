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

namespace MultiTool.Tabs
{
	internal class PlayerTab : Tab
	{
		public override string Name => "Player";

		private Vector2 currentPosition;
		public override void RenderTab(Dimensions dimensions)
		{
			float startingX = dimensions.x + 10f;
			float x = startingX;
			float y = dimensions.y + 10f;
			float buttonWidth = 200f;
			float buttonHeight = 20f;
			float sliderWidth = 300f;
			float headerWidth = dimensions.width - 20f;
			float headerHeight = 40f;

			float scrollHeight = 100f;

			currentPosition = GUI.BeginScrollView(new Rect(x, y, dimensions.width - 20f, dimensions.height - 20f), currentPosition, new Rect(0, 0, dimensions.width - 20f, scrollHeight), new GUIStyle(), GUI.skin.verticalScrollbar);

			GUI.EndScrollView();
		}
	}
}
