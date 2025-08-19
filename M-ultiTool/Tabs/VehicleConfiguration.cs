using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiTool.Modules;
using Logger = MultiTool.Modules.Logger;
using System.Threading;
using static mainscript;
using MultiTool.Utilities.UI;

namespace MultiTool.Tabs
{
	internal class VehicleConfigurationTab : Tab
	{
		public override string Name => "Vehicle Configuration";

		private TabController _tabs = new TabController();

        private Vector2 _position;
        
        private int lastCarId = 0;

        public override void OnRegister()
		{
            _tabs.AddTab(new VehicleConfiguration.Basics());
			_tabs.AddTab(new VehicleConfiguration.Fluids());
			_tabs.AddTab(new VehicleConfiguration.Glass());
			_tabs.AddTab(new VehicleConfiguration.MaterialChanger());
			_tabs.AddTab(new VehicleConfiguration.RandomisedChanger());
			_tabs.AddTab(new VehicleConfiguration.LightChanger());
			_tabs.AddTab(new VehicleConfiguration.EngineTuning());
			_tabs.AddTab(new VehicleConfiguration.TransmissionTuning());
			_tabs.AddTab(new VehicleConfiguration.VehicleTuning());
			_tabs.AddTab(new VehicleConfiguration.WheelTuning());
		}

		public override void Update()
		{
			_tabs.Update();
		}

		public override void RenderTab(Rect dimensions)
		{
            float tabX = dimensions.x + 10f;
            float tabY = dimensions.y + 10f;
            float tabWidth = (dimensions.width - 20f) * 0.11f;

			if (mainscript.M.player.Car == null)
			{
				GUILayout.BeginArea(dimensions);
				GUILayout.FlexibleSpace();
				GUILayout.Label("No current vehicle\nSit in a vehicle to show configuration", "LabelMessage");
				GUILayout.FlexibleSpace();
				GUILayout.EndArea();
				return;
			}

			carscript car = mainscript.M.player.Car;
			tosaveitemscript save = car.GetComponent<tosaveitemscript>();

            // Reset any selections when changing car.
            if (save.idInSave != lastCarId)
            {
				for (int tabIndex = 0; tabIndex < _tabs.GetCount(); tabIndex++)
				{
					Core.VehicleConfigurationTab tab = _tabs.GetByIndex<Core.VehicleConfigurationTab>(tabIndex);
					tab.OnVehicleChange();
				}
			}

            GUILayout.BeginArea(new Rect(tabX, tabY, tabWidth, dimensions.height - 20f));
            GUILayout.BeginVertical("box");

			_position = GUILayout.BeginScrollView(_position);

			for (int tabIndex = 0; tabIndex < _tabs.GetCount(); tabIndex++)
			{
				Tab tab = _tabs.GetByIndex(tabIndex);

				// Ignore any tabs excluded from navigation.
				if (!tab.ShowInNavigation) continue;

				// Render disabled tabs as unclickable.
				if (tab.IsDisabled)
					GUI.enabled = false;

				if (GUILayout.Button(Accessibility.GetAccessibleString(tab.Name, _tabs.GetActive() == tab.Id, true), GUILayout.MinWidth(60), GUILayout.MaxHeight(30)))
					_tabs.SetActive(tab.Id);

				GUI.enabled = true;
			}
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();

			_tabs.RenderTab(dimensions: new Rect(tabX + tabWidth + 10f, tabY, dimensions.width - tabWidth - 10f, dimensions.height - 20f));

            lastCarId = save.idInSave;
		}
	}
}
