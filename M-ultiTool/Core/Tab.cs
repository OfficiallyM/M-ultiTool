using UnityEngine;

namespace MultiTool.Core
{
	public abstract class Tab
	{
		public virtual string Name { get; set; }
		public virtual bool HasConfigPane { get { return false; } }
        public virtual bool ShowInNavigation { get { return true; } }
        internal virtual bool IsFullScreen { get { return false; } }
		public virtual void RenderTab(Rect dimensions) { }
		public virtual void RenderConfigPane(Rect dimensions) { }
        public virtual void OnRegister() { }
        public virtual void OnUnregister() { }
        public virtual void Update() { }

		internal virtual string Source { get; set; }
		internal virtual string Id { get; set; }

		bool disabled = false;
		int errors = 0;
		internal virtual bool IsDisabled { get => disabled; set => disabled = value; }
		internal virtual int Errors { get => errors; set => errors = value; }
	}
}
