using MultiTool.Data;
using MultiTool.Extensions;
using MultiTool.Services;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.UI.Tabs
{
	internal class VehiclesTab : Tab
	{
		public override string Name => "Vehicles";
		public override bool HasConfigPane => true;
		private string _configTitle = "Configuration";
		public override string ConfigTitle => _configTitle;
		private ItemSpawnConfig _spawnConfig = new ItemSpawnConfig();
		public override ISpawnConfig SpawnConfig => _spawnConfig;

		// Scroll vectors.
		private Vector2 _vehicleScrollPosition;
		private Vector2 _configScrollPosition;

		// Main tab variables.
		private Rect _dimensions;
		private string _search = string.Empty;
		private string _lastSearch = string.Empty;
		private float _lastWidth = 0;
		private List<List<Item>> _vehiclesChunked = new List<List<Item>>();
		private bool _rechunk = false;

		private bool _showSpawnHistory = false;
		private List<GameObject> _spawnedObjects = new List<GameObject>();

		public override void OnRegister()
		{
			_spawnConfig.MaxFuelType = (int)Enum.GetValues(typeof(mainscript.fluidenum)).Cast<mainscript.fluidenum>().Max();
			_spawnConfig.MaxCondition = (int)Enum.GetValues(typeof(Condition)).Cast<Condition>().Max();
		}

		public override void OnUnregister()
		{
			_spawnConfig.FuelValues.Clear();
			_spawnConfig.FuelTypes.Clear();
		}

		public override void Update()
		{
			List<Item> vehicles = Services.Database.Vehicles;
			if (_search != _lastSearch)
			{
				vehicles = Services.Database.Vehicles.Where(v => v.Name.ToLower().Contains(_search.ToLower()) || v.GameObject.name.ToLower().Contains(_search.ToLower())).ToList();
				_rechunk = true;
				_lastSearch = _search;
				_vehicleScrollPosition = new Vector2(0, 0);
			}

			if (_lastWidth != _dimensions.width || _rechunk)
			{
				int rowLength = Mathf.FloorToInt(_dimensions.width / 150f);
				_vehiclesChunked = vehicles.ChunkBy(rowLength);
				_lastWidth = _dimensions.width;

				_rechunk = false;
			}

			_configTitle = _showSpawnHistory ? "Spawn history" : "Configuration";
		}

		public override void RenderTab(Rect dimensions)
		{
			_dimensions = dimensions;

			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Search:", GUILayout.MaxWidth(50));
			GUILayout.Space(5);
			_search = GUILayout.TextField(_search, GUILayout.MaxWidth(500));
			GUILayout.Space(5);
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(70)))
			{
				_search = string.Empty;
				_lastSearch = string.Empty;
				_rechunk = true;
			}

			GUILayout.FlexibleSpace();

			MultiTool.Tools.RenderControl("delete_mode");

			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			_vehicleScrollPosition = GUILayout.BeginScrollView(_vehicleScrollPosition);
			foreach (List<Item> vehiclesRow in _vehiclesChunked)
			{
				GUILayout.BeginHorizontal();
				foreach (Item vehicle in vehiclesRow)
				{
					GUILayout.Box("", "button", GUILayout.Width(140), GUILayout.Height(140));
					Rect boxRect = GUILayoutUtility.GetLastRect();
					bool buttonImage = GUI.Button(new Rect(boxRect.x + 10f, boxRect.y - 10f, boxRect.width - 20f, boxRect.height - 20f), vehicle.Thumbnail, "ButtonTransparent");
					bool buttonText = GUI.Button(new Rect(boxRect.x, boxRect.y + (boxRect.height / 2), boxRect.width, boxRect.height / 2), vehicle.Name, "ButtonTransparent");
					if (buttonImage || buttonText)
					{
						GameObject spawned = SpawnUtilities.Spawn(vehicle, _spawnConfig);

						if (spawned != null)
							_spawnedObjects.Add(spawned);
					}
					GUILayout.Space(5);
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		public override void RenderConfigPane(Rect dimensions)
		{
			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			GUILayout.Space(10);
			_configScrollPosition = GUILayout.BeginScrollView(_configScrollPosition);

			if (_showSpawnHistory)
			{
				if (GUILayout.Button("Switch to configuration"))
				{
					_showSpawnHistory = !_showSpawnHistory;
					_configScrollPosition = Vector2.zero;
				}
				GUILayout.Space(10);

				if (_spawnedObjects.Count == 0)
				{
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Nothing has been spawned yet");
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}

				foreach (GameObject obj in _spawnedObjects)
				{
					try
					{
						if (obj == null)
						{
							continue;
						}

						bool isVehicle = GameUtilities.IsVehicleOrTrailer(obj);

						if (!isVehicle) continue;

						string name = obj.name.Prettify();
						name = Services.Translator.T($"vehicle.{name.ToKey()}", name);

						GUILayout.Label(name);
						GUILayout.BeginHorizontal();

						if (mainscript.M.player.Car != null && mainscript.M.player.Car.gameObject == obj)
						{
							GUILayout.Label("Cannot manipulate vehicle you're sitting in");
						}
						else
						{
							if (GUILayout.Button("Teleport to", GUILayout.MaxWidth(100)))
								GameUtilities.TeleportPlayerWithParent(obj.transform.position + Vector3.up * 2f);

							GUILayout.Space(5);

							if (GUILayout.Button("Teleport here", GUILayout.MaxWidth(100)))
							{
								Vector3 position = mainscript.M.player.lookPoint + Vector3.up * 0.75f;
								Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.mainCam.transform.right);

								obj.transform.position = position;
								obj.transform.rotation = rotation;
							}

							GUILayout.Space(5);

							if (GUILayout.Button("Delete", GUILayout.MaxWidth(100)))
							{
								tosaveitemscript save = obj.GetComponent<tosaveitemscript>();
								if (save != null)
								{
									save.removeFromMemory = true;

									foreach (tosaveitemscript component in obj.transform.root.GetComponentsInChildren<tosaveitemscript>())
									{
										component.removeFromMemory = true;
									}
									UnityEngine.Object.Destroy(obj);
									_spawnedObjects.Remove(obj);
									break;
								}
							}
						}

						GUILayout.EndHorizontal();
						GUILayout.Space(10);
					}
					catch
					{
						_spawnedObjects.Remove(obj);
						break;
					}
				}
			}
			else
			{
				if (GUILayout.Button("Switch to spawn history"))
				{
					_showSpawnHistory = !_showSpawnHistory;
					_configScrollPosition = Vector2.zero;
				}
				GUILayout.Space(10);

				SpawnConfig?.Render(dimensions);
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
