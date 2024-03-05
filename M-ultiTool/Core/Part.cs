using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTool.Core
{
	internal class PartGroup
	{
		internal string name;
		internal List<partconditionscript> parts;

		internal static PartGroup Create(string name, List<partconditionscript> parts)
		{
			return new PartGroup()
			{
				name = name,
				parts = parts
			};
		}

		internal static PartGroup Create(string name, partconditionscript part)
		{
			return new PartGroup()
			{
				name = name,
				parts = new List<partconditionscript>() { part }
			};
		}
	}
}
