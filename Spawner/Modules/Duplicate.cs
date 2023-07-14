using SpawnerTLD.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpawnerTLD.Modules
{
	internal class Duplicate
	{
		private Logger logger;
		private Keybinds binds;
		private GameObject dupe = null;

		private Settings settings = new Settings();

		public Duplicate(Logger _logger, Keybinds _binds)
		{
			logger = _logger;
			binds = _binds;
		}

		public void Update()
		{
			if (settings.duplicateMode)
			{
				if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.duplicateClear).key))
					dupe = null;

				if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.duplicateCopy).key))
				{
					Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out var raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
					if (raycastHit.transform.gameObject.GetComponent<tosaveitemscript>() != null)
						dupe = raycastHit.transform.root.gameObject;
				}

				if (Input.GetKeyDown(binds.GetKeyByAction((int)Keybinds.Inputs.duplicatePaste).key) && HasCopy())
				{
					try
					{
						// TODO: Attachables don't work properly. Vehicles spawned and then deleted make the original go pink and see through...?
						GameObject spawned = UnityEngine.Object.Instantiate(dupe, mainscript.M.player.lookPoint + Vector3.up * 0.75f, Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right));
						tosaveitemscript save = spawned.GetComponent<tosaveitemscript>();
						save.FStart();
						if (spawned.GetComponent<childunparent>() != null)
						{
							save = spawned.GetComponent<childunparent>().g.GetComponent<tosaveitemscript>();
							if (save != null)
								save.FStart();
						}
					}
					catch (Exception ex)
					{
						logger.Log($"Error Instantiating dupe - {ex}", Logger.LogLevel.Error);
					}
				}
			}
		}

		/// <summary>
		/// Whether anything is currently copied
		/// </summary>
		/// <returns>True if the clipboard is not empty</returns>
		public bool HasCopy()
		{
			if (dupe != null)
				return true;
			return false;
		}

		/// <summary>
		/// Get the name of the current copied object
		/// </summary>
		/// <returns>String name of the object</returns>
		public string GetDupeName()
		{
			if (HasCopy())
				return dupe.name;
			return string.Empty;
		}
	}
}
