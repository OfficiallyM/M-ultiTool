using System;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Save.Records
{
	internal class WeaponRecord : SaveRecord, ISaveApplier
	{
		public float FireRate { get; set; }
		public float DefaultFireRate { get; set; }

		public void Apply(tosaveitemscript save)
		{
			try
			{
				var weapon = save.GetComponent<weaponscript>();
				if (weapon == null) return;
				weapon.minShootTime = FireRate;
			}
			catch (Exception ex)
			{
				Logger.Log($"Weapon load error - {ex}", Logger.LogLevel.Error);
			}
		}
	}
}
