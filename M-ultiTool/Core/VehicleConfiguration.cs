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

	internal class FluidPercentage
	{
		public mainscript.fluidenum type;
		public float percentage;

		public FluidPercentage Clone()
		{
			return new FluidPercentage()
			{
				type = type,
				percentage = percentage
			};
		}
	}

	internal class FluidMix
	{
		public tankscript tank;
		public List<FluidPercentage> fluids;
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
        [DataMember] internal float startChance;
        [DataMember] internal float motorBrakeModifier;
        [DataMember] internal float minOptimalTemp2;
        [DataMember] internal float maxOptimalTemp2;
        [DataMember] internal float engineHeatGainMin;
        [DataMember] internal float engineHeatGainMax;
        [DataMember] internal bool noOverheat;
        [DataMember] internal bool twoStroke;
        [DataMember] internal mainscript.fluidenum oilFluid;
        [DataMember] internal float oilTolerationMin;
        [DataMember] internal float oilTolerationMax;
        [DataMember] internal float oilConsumptionModifier;
        [DataMember] internal float consumptionModifier;
        [DataMember] internal List<Fluid> consumption = new List<Fluid>();
        [DataMember] internal List<TorqueCurve> torqueCurve = new List<TorqueCurve>();
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
		[DataMember] internal float differentialRatio;
		[DataMember] internal Drivetrain driveTrain;
	}

    [DataContract]
    internal class VehicleTuning
    {
        [DataMember] internal float steerAngle;
        [DataMember] internal float brakePower;    
    }

	internal enum Drivetrain
	{
		FWD,
		RWD,
		AWD,
	}

	[DataContract]
	internal class Wheel
	{
		internal tosaveitemscript save;
		internal wheelgraphicsscript graphics;
		internal string name;
		[DataMember] internal int ID;

		// Grip.
		[DataMember] internal float? forwardSlip;
		[DataMember] internal float? sideSlip;
		[DataMember] internal float wheelDamping;

		// Suspension.
		[DataMember] internal float distance;
		[DataMember] internal float stiffness;
		[DataMember] internal float damper;
		[DataMember] internal float targetPosition;

		// Position.
		[DataMember] internal Vector3 position;
		[DataMember] internal float outwardOffset = 0;
		[DataMember] internal float forwardOffset = 0;
		[DataMember] internal float verticalOffset = 0;
	}

    [DataContract]
    internal class WheelTuning
    {
		[DataMember] internal bool applyToAll = true;
		[DataMember] internal List<Wheel> wheels = new List<Wheel>();
	}
}
