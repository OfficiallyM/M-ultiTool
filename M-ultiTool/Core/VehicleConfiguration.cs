using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace MultiTool.Core
{
	internal class PartGroupParent
	{
		public string name;
		public List<PartGroup> parts;

		public static PartGroupParent Create(string _name)
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
		public string name;
		public string parent;
		public int index;
		public List<partconditionscript> parts;
		public List<MeshRenderer> meshes;

		public static PartGroup Create(string _name, List<partconditionscript> _parts, int _index, string _parent)
		{
			return new PartGroup()
			{
				name = _name,
				parts = _parts,
				index = _index,
				parent = _parent,
			};
		}

		public static PartGroup Create(string _name, partconditionscript _part, int _index, string _parent)
		{
            return Create(_name, new List<partconditionscript>() { _part }, _index, _parent);
		}

		public static PartGroup Create(string _name, List<MeshRenderer> _meshes, int _index, string _parent)
        {
            return new PartGroup()
            {
                name = _name,
                meshes = _meshes,
				index = _index,
				parent = _parent,
			};
        }

		public static PartGroup Create(string _name, MeshRenderer _mesh, int _index, string _parent)
        {
            return Create(_name, new List<MeshRenderer>() { _mesh }, _index, _parent);
        }

		public bool IsConditionless()
        {
            return meshes != null && meshes.Count > 0 && (parts == null || parts.Count == 0);
        }
    }

	internal class LightGroup
	{
		public string name;
		public List<headlightscript> headlights;
		public bool isInteriorLight;

		public static LightGroup Create(string _name, List<headlightscript> _headlights = null, bool _isInteriorLight = false)
		{
			return new LightGroup()
			{
				name = _name,
				headlights = _headlights,
				isInteriorLight = _isInteriorLight
			};
		}

		public static LightGroup Create(string _name, headlightscript _headlight, bool _isInteriorLight = false)
		{
			return Create(_name, new List<headlightscript>() { _headlight }, _isInteriorLight);
		}
	}

	[DataContract]
	internal class TorqueCurve
    {
        [DataMember] public float torque;
        [DataMember] public float rpm;

		public TorqueCurve(float _torque, float _rpm)
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

	internal class TankCapacity
	{
		public tankscript tank;
		public float max;
		public float defaultMax;
	}

	// Placeholder interface to allow for generic tuning saving.
	internal interface ITuning { }

	[DataContract]
	[KnownType("GetKnownTypes")]
	internal class TuningSave
	{
		[DataMember] public string name;
		[DataMember] public string type;
		[DataMember] public ITuning tuning;

		private static IEnumerable<Type> _knownTypes;
		private static IEnumerable<Type> GetKnownTypes()
		{
			if (_knownTypes == null)
				_knownTypes = Assembly.GetExecutingAssembly()
					.GetTypes()
					.Where(t => typeof(ITuning).IsAssignableFrom(t))
					.ToList();
			return _knownTypes;
		}
	}

	internal class EngineStats
    {
		public float maxTorque;
		public float maxRPM;
		public float maxHp;
		public Texture2D torqueGraph;
    }

    [DataContract]
	internal class EngineTuning : ITuning
    {
        [DataMember] public float rpmChangeModifier;
        [DataMember] public float startChance;
        [DataMember] public float motorBrakeModifier;
        [DataMember] public float minOptimalTemp2;
        [DataMember] public float maxOptimalTemp2;
        [DataMember] public float engineHeatGainMin;
        [DataMember] public float engineHeatGainMax;
        [DataMember] public bool noOverheat;
        [DataMember] public bool twoStroke;
        [DataMember] public mainscript.fluidenum oilFluid;
        [DataMember] public float oilTolerationMin;
        [DataMember] public float oilTolerationMax;
        [DataMember] public float oilConsumptionModifier;
        [DataMember] public float consumptionModifier;
        [DataMember] public List<Fluid> consumption = new List<Fluid>();
        [DataMember] public List<TorqueCurve> torqueCurve = new List<TorqueCurve>();
    }

    [DataContract]
	internal class Gear
    {
        [DataMember] public int gear;
        [DataMember] public float ratio;
        [DataMember] public bool freeRun;

		public Gear(int _gear, float _ratio, bool _freeRun)
        {
            gear = _gear;
            ratio = _ratio;
            freeRun = _freeRun;
        }
    }

    [DataContract]
	internal class TransmissionTuning : ITuning
    {
        [DataMember] public List<Gear> gears = new List<Gear>();
		[DataMember] public float differentialRatio;
		[DataMember] public Drivetrain driveTrain;
	}

    [DataContract]
	internal class VehicleTuning : ITuning
    {
        [DataMember] public float steerAngle;
        [DataMember] public float brakePower;    
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
		public tosaveitemscript save;
		public wheelgraphicsscript graphics;
		[DataMember] public string slot;

		// Grip.
		[DataMember] public float? forwardSlip;
		[DataMember] public float? sideSlip;
		[DataMember] public float wheelDamping;

		// Suspension.
		[DataMember] public float distance;
		[DataMember] public float stiffness;
		[DataMember] public float damper;
		[DataMember] public float targetPosition;

		// Position.
		[DataMember] public Vector3 position;
		[DataMember] public float outwardOffset = 0;
		[DataMember] public float forwardOffset = 0;
		[DataMember] public float verticalOffset = 0;
	}

    [DataContract]
    internal class WheelTuning : ITuning
    {
		[DataMember] public bool applyToAll = true;
		[DataMember] public List<Wheel> wheels = new List<Wheel>();
	}
}
