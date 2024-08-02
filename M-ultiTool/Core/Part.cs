using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Core
{
	internal class PartGroup
	{
		internal string name;
		internal List<partconditionscript> parts;
        internal List<MeshRenderer> meshes;

		internal static PartGroup Create(string _name, List<partconditionscript> _parts)
		{
			return new PartGroup()
			{
				name = _name,
				parts = _parts,
			};
		}

		internal static PartGroup Create(string _name, partconditionscript _part)
		{
            return Create(_name, new List<partconditionscript>() { _part });
		}

        internal static PartGroup Create(string _name, List<MeshRenderer> _meshes)
        {
            return new PartGroup()
            {
                name = _name,
                meshes = _meshes,
            };
        }

        internal static PartGroup Create(string _name, MeshRenderer _mesh)
        {
            return Create(_name, new List<MeshRenderer>() { _mesh });
        }

        internal bool IsConditionless()
        {
            return meshes != null && meshes.Count > 0 && (parts == null || parts.Count == 0);
        }
    }

    internal class LightGroup
    {
        internal string name;
        internal List<headlightscript> headlights;
        internal bool isInteriorLight;

        internal static LightGroup Create(string _name, List<headlightscript> _headlights = null, bool _isInteriorLight = false)
        {
            return new LightGroup()
            {
                name = _name,
                headlights = _headlights,
                isInteriorLight = _isInteriorLight
            };
        }

        internal static LightGroup Create(string _name, headlightscript _headlight, bool _isInteriorLight = false)
        {
            return Create(_name, new List<headlightscript>() { _headlight }, _isInteriorLight);
        }
    }
}
