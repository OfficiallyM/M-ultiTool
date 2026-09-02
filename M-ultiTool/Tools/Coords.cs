using MultiTool.UI;
using MultiTool.Utilities;
using UnityEngine;

namespace MultiTool.Tools
{
	internal class CoordsTool : Tool
	{
		public override string Name => "Show coords";
		public override bool IsExclusive => false;

		private readonly GUIStyle _hudStyle = new GUIStyle()
		{
			fontSize = 20,
			alignment = TextAnchor.MiddleLeft,
			normal = new GUIStyleState()
			{
				textColor = Color.white,
			}
		};

		public override void ControlRender()
		{
			if (GUILayout.Button(Tools.GetAccessibleName(Id), GUILayout.MaxWidth(200)))
				Tools.Toggle(Id);
		}

		public override void HudRender()
		{
			GUIExtensions.DrawOutline(new Rect(20f, 20f, 600f, 30f), $"Local position: {mainscript.M.player.transform.position}", _hudStyle, Color.black);
			GUIExtensions.DrawOutline(new Rect(20f, 50f, 600f, 30f), $"Global position: {GameUtilities.GetGlobalObjectPosition(mainscript.M.player.transform.position)}", _hudStyle, Color.black);
		}
	}
}
