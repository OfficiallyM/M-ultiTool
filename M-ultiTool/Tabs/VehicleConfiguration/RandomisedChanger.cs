using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static partslotscript;

namespace MultiTool.Tabs.VehicleConfiguration
{
    internal sealed class RandomisedChanger : Core.VehicleConfigurationTab
	{
        public override string Name => "Randomised Changer";

		private Vector2 _position;
		private carscript _car;

		private List<randomTypeSelector> _randomParts = new List<randomTypeSelector>();
		private bool _randomSelectorOpen = false;
		private randomTypeSelector _selectedRandom = null;

		public override void OnCacheRefresh()
		{
			if (_car == null)
				_car = mainscript.s.player.Car;

			_randomParts.Clear();
			_randomParts = _car.GetComponentsInChildren<randomTypeSelector>().ToList();
		}
		public override void OnVehicleChange()
		{
			_selectedRandom = null;
		}

		public override void RenderTab(Rect dimensions)
		{
			dimensions.width /= 2;
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			_position = GUILayout.BeginScrollView(_position);

			carscript car = mainscript.s.player.Car;

			GUILayout.Label("Randomiser changer", "LabelHeader");

			// Random selector.
			string randomSelectString = "Select randomised part";
			if (_selectedRandom != null)
				randomSelectString = $"Selected: {PrettifyName(_selectedRandom.name)}";
			if (GUILayout.Button(randomSelectString, GUILayout.MaxWidth(200)))
				_randomSelectorOpen = !_randomSelectorOpen;

			if (_randomSelectorOpen)
			{
				if (GUILayout.Button("None", GUILayout.MaxWidth(200)))
				{
					_selectedRandom = null;
					_randomSelectorOpen = false;
				}
				foreach (randomTypeSelector random in _randomParts)
				{
					if (GUILayout.Button(PrettifyName(random.name), GUILayout.MaxWidth(200)))
					{
						_selectedRandom = random;
						_randomSelectorOpen = false;
					}
				}
			}

			if (_selectedRandom != null)
			{
				for (int i = 0; i < _selectedRandom.tipusok.Length; i++)
				{
					if (GUILayout.Button($"Option {i + 1}", GUILayout.MaxWidth(200)))
					{
						_selectedRandom.rtipus = i;
						_selectedRandom.Refresh();
					}
				}
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		/// <summary>
		/// Make part name more user friendly.
		/// </summary>
		/// <param name="random">Part name to prettify</param>
		/// <returns>Prettified part name</returns>
		private string PrettifyName(string name)
		{
			return name.Replace("(Clone)", string.Empty);
		}
	}
}
