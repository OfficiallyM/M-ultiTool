using MultiTool.Services;

namespace MultiTool.Tools
{
	internal abstract class Tool
	{
		internal ServiceContext Services { get; set; }
		internal ToolController Tools { get; set; }

		
		public abstract string Name { get; }
		public virtual bool HasCache { get { return false; } }
		public virtual int CacheRefreshTime { get { return 1; } }
		public virtual void OnRegister() { }
		public virtual void OnUnregister() { }
		public virtual void OnActivate() { }
		public virtual void OnDeactivate() { }
		public virtual void Update() { }
		public virtual void FixedUpdate() { }
		public virtual void OnCacheRefresh() { }
		public virtual void HudRender() { }
		public virtual void ControlRender() { }

		internal virtual string Source { get; set; }
		internal virtual string Id { get; set; }

		private float _nextCacheUpdate = 0;
		internal virtual float NextCacheUpdate { get => _nextCacheUpdate; set => _nextCacheUpdate = value; }
	}
}
