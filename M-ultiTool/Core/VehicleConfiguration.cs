using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Core
{
	internal class PartGroupParent
	{
		internal string name;
		internal List<PartGroup> parts;

		internal static PartGroupParent Create(string _name)
		{
			return new PartGroupParent()
			{
				name = _name, 
				parts = new List<PartGroup>(),
			};
		}
	}

	internal class PartGroup
	{
		internal string name;
		internal int index;
		internal List<partconditionscript> parts;
        internal List<MeshRenderer> meshes;

		internal static PartGroup Create(string _name, List<partconditionscript> _parts, int _index)
		{
			return new PartGroup()
			{
				name = _name,
				parts = _parts,
				index = _index
			};
		}

		internal static PartGroup Create(string _name, partconditionscript _part, int _index)
		{
            return Create(_name, new List<partconditionscript>() { _part }, _index);
		}

        internal static PartGroup Create(string _name, List<MeshRenderer> _meshes, int _index)
        {
            return new PartGroup()
            {
                name = _name,
                meshes = _meshes,
				index = _index
			};
        }

        internal static PartGroup Create(string _name, MeshRenderer _mesh, int _index)
        {
            return Create(_name, new List<MeshRenderer>() { _mesh }, _index);
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

	[DataContract]
    internal class TorqueCurve
    {
        [DataMember] internal float torque;
        [DataMember] internal float rpm;

        internal TorqueCurve(float _torque, float _rpm)
        {
            torque = _torque;
            rpm = _rpm;
        }
    }

    // This is required to serialize the data in the existing save system.
    [DataContract]
    internal class Fluid
    {
        [DataMember] public mainscript.fluidenum type;
        [DataMember] public float amount;
    }

    internal class EngineStats
    {
        internal float maxTorque;
        internal float maxRPM;
        internal float maxHp;
        internal Texture2D torqueGraph;
    }

    [DataContract]
    internal class EngineTuning
    {
        [DataMember] internal float rpmChangeModifier;
        [DataMember] internal float defaultRpmChangeModifier;

        [DataMember] internal float startChance;
        [DataMember] internal float defaultStartChance;

        [DataMember] internal float motorBrakeModifier;
        [DataMember] internal float defaultMotorBrakeModifier;

        [DataMember] internal float minOptimalTemp2;
        [DataMember] internal float defaultMinOptimalTemp2;

        [DataMember] internal float maxOptimalTemp2;
        [DataMember] internal float defaultMaxOptimalTemp2;

        [DataMember] internal float engineHeatGainMin;
        [DataMember] internal float defaultEngineHeatGainMin;

        [DataMember] internal float engineHeatGainMax;
        [DataMember] internal float defaultEngineHeatGainMax;

        [DataMember] internal bool noOverheat;
        [DataMember] internal bool defaultNoOverheat;

        [DataMember] internal bool twoStroke;
        [DataMember] internal bool defaultTwoStroke;

        [DataMember] internal mainscript.fluidenum oilFluid;
        [DataMember] internal mainscript.fluidenum defaultOilFluid;

        [DataMember] internal float oilTolerationMin;
        [DataMember] internal float defaultOilTolerationMin;

        [DataMember] internal float oilTolerationMax;
        [DataMember] internal float defaultOilTolerationMax;

        [DataMember] internal float oilConsumptionModifier;
        [DataMember] internal float defaultOilConsumptionModifier;

        [DataMember] internal float consumptionModifier;
        [DataMember] internal float defaultConsumptionModifier;

        [DataMember] internal List<Fluid> consumption = new List<Fluid>();
        [DataMember] internal List<Fluid> defaultConsumption = new List<Fluid>();

        [DataMember] internal List<TorqueCurve> torqueCurve = new List<TorqueCurve>();
        [DataMember] internal List<TorqueCurve> defaultTorqueCurve = new List<TorqueCurve>();
    }

    [DataContract]
    internal class Gear
    {
        [DataMember] internal int gear;
        [DataMember] internal float ratio;
        [DataMember] internal bool freeRun;

        internal Gear(int _gear, float _ratio, bool _freeRun)
        {
            gear = _gear;
            ratio = _ratio;
            freeRun = _freeRun;
        }
    }

    [DataContract]
    internal class TransmissionTuning
    {
        [DataMember] internal List<Gear> gears = new List<Gear>();
        [DataMember] internal List<Gear> defaultGears = new List<Gear>();
    }

    [DataContract]
    internal class VehicleTuning
    {
        [DataMember] internal float steerAngle;
        [DataMember] internal float defaultSteerAngle;

        [DataMember] internal float brakePower;
        [DataMember] internal float defaultBrakePower;

        [DataMember] internal float differentialRatio;
        [DataMember] internal float defaultDifferentialRatio;
    }
}
