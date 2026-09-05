using MultiTool.Services;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Tools
{
	internal abstract class Tool
	{
		public ServiceContext Services { get; set; }
		public ToolController Tools { get; set; }
		public tosaveitemscript SelectedObject { get; set; }

		
		public abstract string Name { get; }
		/// <summary>
		/// Set to false to allow the tool to run in the background
		/// with other tools or true to replace the current exclusive tool
		/// when activated.
		/// </summary>
		public virtual bool IsExclusive { get { return true; } }
		public virtual bool HasCache { get { return false; } }
		public virtual int CacheRefreshTime { get { return 1; } }
		public virtual bool UsesObjectSelection { get { return false; } }
		public virtual bool UsesDefaultObjectSelectionUI { get { return false; } }
		public virtual void OnRegister() { }
		public virtual void OnUnregister() { }
		public virtual void OnActivate() { }
		public virtual void OnDeactivate() { }
		public virtual void Update() { }
		public virtual void FixedUpdate() { }
		public virtual void OnCacheRefresh() { }
		public virtual void HudRender() { }
		public virtual void ControlRender() { }

		public virtual string Id { get; set; }

		public virtual bool IsDisabled { get; set; }
		public virtual int Errors { get; set; }
		public virtual float NextCacheUpdate { get; set; }

		public void IncrementErrors()
		{
			Errors++;

			if (Errors >= 5)
			{
				IsDisabled = true;
				Logger.Log($"{Name} has been disabled for throwing too many errors.", Logger.LogLevel.Error, "ToolController");
				MultiTool.Tools.Deactivate(Id);
			}
		}

		public GameObject Raycast()
		{
			Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out RaycastHit raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
			return raycastHit.collider?.transform?.gameObject;
		}
	}
}
