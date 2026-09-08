using MultiTool.Services;
using System;

namespace MultiTool.Save.Records
{
	internal class TankRecord : SaveRecord, ISaveApplier
	{
		public float Capacity { get; set; }
		public float DefaultCapacity { get; set; }

		public void Apply(tosaveitemscript save)
		{
			try
			{
				tankscript tank = null;
				carscript car = save.GetComponent<carscript>();
				// Support for car fuel tanks.
				if (car != null)
					tank = car.Tank;
				else
					tank = save.GetComponentInChildren<tankscript>();

				if (tank != null)
					tank.F.maxC = Capacity;
			}
			catch (Exception ex)
			{
				Logger.Log($"Tank data load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
