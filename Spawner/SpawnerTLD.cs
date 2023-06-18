using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TLDLoader;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using SpawnerTLD.Core;
using SpawnerTLD.Modules;
using Logger = SpawnerTLD.Modules.Logger;
using Settings = SpawnerTLD.Core.Settings;

namespace SpawnerTLD
{
	public class SpawnerTLD : Mod
	{
		// Mod meta stuff.
		public override string ID => Meta.ID;
		public override string Name => Meta.Name;
		public override string Author => Meta.Author;
		public override string Version => Meta.Version;

		// Initialise modules.
		private Logger logger = new Logger();
		private GUIRenderer renderer;

		private Settings settings = new Settings();

		// Override functions.
		public override void OnGUI()
		{
			renderer.OnGUI();
		}

		public override void OnLoad()
		{
			// Initialise GUI renderer.
			renderer = new GUIRenderer(this);

			// Distance check.
			float minDistance = 1000f;
			float distance = mainscript.DistanceRead();
			if (distance >= minDistance)
				renderer.enabled = true;

			// Return early if spawner is disabled.
			if (!renderer.enabled)
			{
				logger.Log("Distance requirement not met, spawner disabled.", Logger.LogLevel.Warning);
				return;
			}

			renderer.OnLoad();
		}

		public override void Update()
		{
			// Return early if spawner isn't enabled.
			if (!renderer.enabled)
				return;

			if (settings.deleteMode)
			{
				if (Input.GetKeyDown(KeyCode.Delete) && mainscript.M.player.seat == null)
				{
					Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
					raycastHit.transform.gameObject.GetComponent<tosaveitemscript>().removeFromMemory = true;	
					foreach (tosaveitemscript component in raycastHit.transform.root.GetComponentsInChildren<tosaveitemscript>())
					{
						component.removeFromMemory = true;
					}
					UnityEngine.Object.Destroy(raycastHit.transform.root.gameObject);
				}
			}
		}
	}
}