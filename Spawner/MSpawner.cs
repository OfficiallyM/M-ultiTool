using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TLDLoader;

namespace MSpawner
{
    public class MSpawner : Mod
    {
        // Mod meta stuff.
        public override string ID => "MSpawner";
        public override string Name => "Spawner";
        public override string Author => "M-";
        public override string Version => "1.0.0";
		public override bool UseAssetsFolder => true;

        // Variables.
        private bool show;
        private bool enabled;
        private GUIStyle style;
        private GUIStyle smallStyle;
        private float minDistance = 1000f;

        // Override functions.
        public override void OnGUI()
		{
            // Return early if spawner is disabled.
            if (!enabled)
                return;

            if (mainscript.M.menu.Menu.activeSelf)
                ToggleVisibility();
		}

        public override void OnLoad()
		{
            style.fontSize = 14;
            style.font = Font.CreateDynamicFontFromOSFont("Consolas", 14);
            smallStyle.fontSize = 10;
            smallStyle.font = Font.CreateDynamicFontFromOSFont("Consolas", 10);

            // Distance check.
            float distance = mainscript.DistanceRead();
            if (distance >= minDistance)
                enabled = true;


        }

		public override void Update()
		{
		}

		// Mod-specific functions.
        public MSpawner()
		{
            style = new GUIStyle();
		}

		private void ToggleVisibility()
		{
            if (GUI.Button(new Rect(230f, 30f, 50f, 50f), show ? "<size=28><color=green><b>X</b></color></size>" : "<size=28><color=red><b>X</b></color></size>"))
                show = !show;
        }
	}
}