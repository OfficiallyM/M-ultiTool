using UnityEngine;

namespace MultiTool.Core
{
	public abstract class Tab
	{
		public virtual string Name { get; set; }
		public virtual bool HasConfigPane { get { return false; } }
		public virtual string ConfigTitle { get; set; }
		public virtual bool HasCache { get { return false; } }
		public virtual int CacheRefreshTime { get { return 1; } }
        public virtual bool ShowInNavigation { get { return true; } }
        internal virtual bool IsFullScreen { get { return false; } }
		public virtual void RenderTab(Rect dimensions) { }
		public virtual void RenderConfigPane(Rect dimensions) { }
        public virtual void OnRegister() { }
        public virtual void OnUnregister() { }
        public virtual void Update() { }
		public virtual void OnCacheRefresh() { }

		internal virtual string Source { get; set; }
		internal virtual string Id { get; set; }

		private bool _disabled = false;
		private int _errors = 0;
		private float _nextCacheUpdate = 0;
		internal virtual bool IsDisabled { get => _disabled; set => _disabled = value; }
		internal virtual int Errors { get => _errors; set => _errors = value; }
		internal virtual float NextCacheUpdate { get => _nextCacheUpdate; set => _nextCacheUpdate = value; }
	}

	internal abstract class VehicleConfigurationTab : Tab
	{
		public virtual void OnVehicleChange() { }
	}
}
