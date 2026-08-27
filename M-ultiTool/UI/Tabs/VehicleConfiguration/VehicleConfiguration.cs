using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace MultiTool.UI.Tabs.VehicleConfiguration
{
	internal class PartGroupParent
	{
		public string Name;
		public List<PartGroup> Parts;

		public static PartGroupParent Create(string _name)
		{
			return new PartGroupParent()
			{
				Name = _name,
				Parts = new List<PartGroup>(),
			};
		}
	}

	internal class PartGroup
	{
		public string Name;
		public string Parent;
		public int Index;
		public List<partconditionscript> Parts;
		public List<MeshRenderer> Meshes;

		public static PartGroup Create(string _name, List<partconditionscript> _parts, int _index, string _parent)
		{
			return new PartGroup()
			{
				Name = _name,
				Parts = _parts,
				Index = _index,
				Parent = _parent,
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
				Name = _name,
				Meshes = _meshes,
				Index = _index,
				Parent = _parent,
			};
		}

		public static PartGroup Create(string _name, MeshRenderer _mesh, int _index, string _parent)
		{
			return Create(_name, new List<MeshRenderer>() { _mesh }, _index, _parent);
		}

		public bool IsConditionless()
		{
			return Meshes != null && Meshes.Count > 0 && (Parts == null || Parts.Count == 0);
		}
	}

	internal class LightGroup
	{
		public string Name;
		public List<headlightscript> Headlights;
		public bool IsInteriorLight;

		public static LightGroup Create(string _name, List<headlightscript> _headlights = null, bool _isInteriorLight = false)
		{
			return new LightGroup()
			{
				Name = _name,
				Headlights = _headlights,
				IsInteriorLight = _isInteriorLight
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
		[DataMember(Name = "torque")] public float Torque;
		[DataMember(Name = "rpm")] public float Rpm;

		public TorqueCurve(float _torque, float _rpm)
		{
			Torque = _torque;
			Rpm = _rpm;
		}
	}

	// This is required to serialize the data in the existing save system.
	[DataContract]
	internal class Fluid
	{
		[DataMember(Name = "type")] public mainscript.fluidenum Type;
		[DataMember(Name = "amount")] public float Amount;
	}

	internal class FluidPercentage
	{
		public mainscript.fluidenum Type;
		public float Percentage;

		public FluidPercentage Clone()
		{
			return new FluidPercentage()
			{
				Type = Type,
				Percentage = Percentage
			};
		}
	}

	internal class FluidMix
	{
		public tankscript Tank;
		public List<FluidPercentage> Fluids;
	}

	internal class TankCapacity
	{
		public tankscript Tank;
		public float Max;
		public float DefaultMax;
	}

	// Placeholder interface to allow for generic tuning saving.
	internal interface ITuning { }

	[DataContract]
	[KnownType("GetKnownTypes")]
	internal class TuningSave
	{
		[DataMember(Name = "name")] public string Name;
		[DataMember(Name = "part")] public string Part;
		[DataMember(Name = "type")] public string Type;
		[DataMember(Name = "car")] public string Car;
		[DataMember(Name = "tuning")] public ITuning Tuning;

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
		public float MaxTorque;
		public float MaxRPM;
		public float MaxHp;
		public Texture2D TorqueGraph;
	}

	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/MultiTool.Core")]
	internal class EngineTuning : ITuning
	{
		[DataMember(Name = "rpmChangeModifier")] public float RpmChangeModifier;
		[DataMember(Name = "startChance")] public float StartChance;
		[DataMember(Name = "motorBrakeModifier")] public float MotorBrakeModifier;
		[DataMember(Name = "minOptimalTemp2")] public float MinOptimalTemp2;
		[DataMember(Name = "maxOptimalTemp2")] public float MaxOptimalTemp2;
		[DataMember(Name = "engineHeatGainMin")] public float EngineHeatGainMin;
		[DataMember(Name = "engineHeatGainMax")] public float EngineHeatGainMax;
		[DataMember(Name = "noOverheat")] public bool NoOverheat;
		[DataMember(Name = "twoStroke")] public bool TwoStroke;
		[DataMember(Name = "oilFluid")] public mainscript.fluidenum OilFluid;
		[DataMember(Name = "oilTolerationMin")] public float OilTolerationMin;
		[DataMember(Name = "oilTolerationMax")] public float OilTolerationMax;
		[DataMember(Name = "oilConsumptionModifier")] public float OilConsumptionModifier;
		[DataMember(Name = "consumptionModifier")] public float ConsumptionModifier;
		[DataMember(Name = "consumption")] public List<Fluid> Consumption = new List<Fluid>();
		[DataMember(Name = "torqueCurve")] public List<TorqueCurve> TorqueCurve = new List<TorqueCurve>();
	}

	[DataContract]
	internal class Gear
	{
		[DataMember(Name = "gear")] public int GearNumber;
		[DataMember(Name = "ratio")] public float Ratio;
		[DataMember(Name = "freeRun")] public bool FreeRun;

		public Gear(int _gear, float _ratio, bool _freeRun)
		{
			GearNumber = _gear;
			Ratio = _ratio;
			FreeRun = _freeRun;
		}
	}

	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/MultiTool.Core")]
	internal class TransmissionTuning : ITuning
	{
		[DataMember(Name = "gears")] public List<Gear> Gears = new List<Gear>();
		[DataMember(Name = "differentialRatio")] public float DifferentialRatio;
		[DataMember(Name = "driveTrain")] public Drivetrain DriveTrain;
	}

	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/MultiTool.Core")]
	internal class VehicleTuning : ITuning
	{
		[DataMember(Name = "steerAngle")] public float SteerAngle;
		[DataMember(Name = "brakePower")] public float BrakePower;
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
		[JsonIgnore] public tosaveitemscript Save;
		[JsonIgnore] public wheelgraphicsscript Graphics;
		[DataMember(Name = "slot")] public string Slot;

		// Grip.
		[DataMember(Name = "forwardSlip")] public float? ForwardSlip;
		[DataMember(Name = "sideSlip")] public float? SideSlip;
		[DataMember(Name = "wheelDamping")] public float WheelDamping;

		// Suspension.
		[DataMember(Name = "distance")] public float Distance;
		[DataMember(Name = "stiffness")] public float Stiffness;
		[DataMember(Name = "damper")] public float Damper;
		[DataMember(Name = "targetPosition")] public float TargetPosition;

		// Position.
		[DataMember(Name = "position")] public Vector3 Position;
		[DataMember(Name = "outwardOffset")] public float OutwardOffset = 0;
		[DataMember(Name = "forwardOffset")] public float ForwardOffset = 0;
		[DataMember(Name = "verticalOffset")] public float VerticalOffset = 0;
	}

	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/MultiTool.Core")]
	internal class WheelTuning : ITuning
	{
		[DataMember(Name = "applyToAll")] public bool ApplyToAll = true;
		[DataMember(Name = "wheels")] public List<Wheel> Wheels = new List<Wheel>();
	}
}