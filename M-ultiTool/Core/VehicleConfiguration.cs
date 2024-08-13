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

    internal class TorqueCurve
    {
        internal float torque;
        internal float rpm;

        internal TorqueCurve(float _torque, float _rpm)
        {
            torque = _torque;
            rpm = _rpm;
        }
    }

    internal class EngineStats
    {
        internal float maxTorque;
        internal float maxRPM;
        internal float maxHp;
        internal Texture2D torqueGraph;
    }

    internal class EngineTuning
    {
        internal float rpmChangeModifier;
        internal float defaultRpmChangeModifier;

        internal float startChance;
        internal float defaultStartChance;

        internal float motorBrakeModifier;
        internal float defaultMotorBrakeModifier;

        internal float minOptimalTemp2;
        internal float defaultMinOptimalTemp2;

        internal float maxOptimalTemp2;
        internal float defaultMaxOptimalTemp2;

        internal float engineHeatGainMin;
        internal float defaultEngineHeatGainMin;

        internal float engineHeatGainMax;
        internal float defaultEngineHeatGainMax;

        internal bool noOverheat;
        internal bool defaultNoOverheat;

        internal bool twoStroke;
        internal bool defaultTwoStroke;

        internal mainscript.fluidenum oilFluid;
        internal mainscript.fluidenum defaultOilFluid;

        internal float oilTolerationMin;
        internal float defaultOilTolerationMin;

        internal float oilTolerationMax;
        internal float defaultOilTolerationMax;

        internal float oilConsumptionModifier;
        internal float defaultOilConsumptionModifier;

        internal float consumptionModifier;
        internal float defaultConsumptionModifier;

        internal List<mainscript.fluid> consumption = new List<mainscript.fluid>();
        internal List<mainscript.fluid> defaultConsumption = new List<mainscript.fluid>();

        internal List<TorqueCurve> torqueCurve = new List<TorqueCurve>();
        internal List<TorqueCurve> defaultTorqueCurve = new List<TorqueCurve>();
    }
}
