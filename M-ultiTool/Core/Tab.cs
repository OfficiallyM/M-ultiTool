using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTool.Core
{
	public abstract class Tab
	{
		public virtual string Name { get; set; }
		public virtual bool HasConfigPane { get { return false; } }
		public virtual void RenderTab(Dimensions dimensions) { }
		public virtual void RenderConfigPane(Dimensions dimensions) { }

		internal virtual string Source { get; set; }
		internal virtual int Id { get; set; }

		bool disabled = false;
		int errors = 0;
		internal virtual bool IsDisabled { get => disabled; set => disabled = value; }
		internal virtual int Errors { get => errors; set => errors = value; }
	}

	public class Dimensions
	{
		public float x;
		public float y;
		public float width;
		public float height;
	}
}
