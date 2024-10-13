using MultiTool.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiTool.Tabs
{
	internal class CreditsTab : Tab
	{
		public override string Name => "Credits";
        public override bool ShowInNavigation => false;
        internal override bool IsFullScreen => true;

		private Vector2 currentPosition;
		public override void RenderTab(Rect dimensions)
		{
            float creditsX = dimensions.x + 10f;
            float creditsY = dimensions.y;
            float creditsWidth = dimensions.width;
            float creditsHeight = dimensions.height;
            GUI.Box(new Rect(creditsX, creditsY, creditsWidth, creditsHeight), "<size=18>Credits</size>");

            creditsX += 10f;
            creditsY += 50f;

            List<string> credits = new List<string>()
                {
                    "M- - Maintainer",
                    "RUNDEN - Thumbnail generator",
                    "FreakShow - Original spawner",
                    "_RainBowSheep_ - Original spawner",
                    "Jessica - New mod name suggestion",
                };

            List<string> other = new List<string>()
                {
                    "copperboltwire",
                    "SgtJoe",
                    "Tumpy_Noodles",
                    "SubG",
                    "_Starixx",
                    "Sabi",
                    "Jessica",
                    "Doubt",
                    "dela",
                    "DummyModder",
                    "Cerulean",
                    "Cassidy",
                    "Runden",
                    "Ghaleas",
                    "PPSz",
                    "Egerdion",
                    "Platinum",
                    "Iron",
                    "sinNeer",
                };

            float totalCreditsHeight = (credits.Count + other.Count) * 35f;

            currentPosition = GUI.BeginScrollView(new Rect(creditsX, creditsY, creditsWidth, creditsHeight), currentPosition, new Rect(0, 0, creditsWidth, totalCreditsHeight));

            float y = 0;
            float x = 0;
            foreach (string credit in credits)
            {
                GUI.Label(new Rect(x, y, creditsWidth, 30f), $"<size=16>{credit}</size>");
                y += 30f;
            }

            y += 30f;

            GUI.Label(new Rect(x, y, creditsWidth, 30f), $"<b><size=16>With special thanks to the following for the bug reports/feature suggestions:</size></b>");
            y += 35f;
            foreach (string name in other)
            {
                GUI.Label(new Rect(x, y, creditsWidth, 30f), $"<size=16>{name}</size>");
                y += 30f;
            }
            GUI.EndScrollView();
        }
	}
}
