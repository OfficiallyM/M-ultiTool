using MultiTool.Services;
using MultiTool.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Tools
{
	internal sealed class ToolController
	{
		private readonly ServiceContext _services;

		private Tool _active;
		private List<Tool> _tools = new List<Tool>();
		private List<Tool> _cacheTools = new List<Tool>();

		public ToolController(ServiceContext services)
		{
			_services = services;
		}

		public void Update()
		{
			if (MultiTool.Renderer.Show) return;

			// Handle object selection if tool calls for it.
			if (_active != null && _active.UsesObjectSelection)
			{
				if (Input.GetKeyDown(_services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action1).AssignedKey))
				{
					Physics.Raycast(mainscript.M.player.Cam.transform.position, mainscript.M.player.Cam.transform.forward, out RaycastHit raycastHit, float.PositiveInfinity, mainscript.M.player.useLayer);
					if (raycastHit.collider?.transform?.gameObject != null)
					{
						GameObject hitGameObject = raycastHit.collider.transform.gameObject;
						tosaveitemscript save = hitGameObject?.GetComponentInParent<tosaveitemscript>();
						bool isTerrain = save?.GetComponent<terrainscript>() != null;

						if (save != null && !isTerrain)
							_active.SelectedObject = save;
						else
							_active.SelectedObject = null;
					}
					else
					{
						_active.SelectedObject = null;
					}
				}
			}

			try
			{
				_active?.Update();
			}
			catch (Exception ex)
			{
				Logger.Log($"{_active.Name} - error during Update(). Details: {ex}", Logger.LogLevel.Error, "ToolController");
				_active.IncrementErrors();
			}

			foreach (Tool tool in _cacheTools)
			{
				if (!tool.HasCache || tool.IsDisabled) continue;
				tool.NextCacheUpdate -= Time.unscaledDeltaTime;
				if (tool.NextCacheUpdate <= 0)
				{
					try
					{
						tool.OnCacheRefresh();
					}
					catch (Exception ex)
					{
						Logger.Log($"{tool.Name} - error during OnCacheRefresh(). Details: {ex}", Logger.LogLevel.Error, "ToolController");
						tool.IncrementErrors();
					}
					tool.NextCacheUpdate = tool.CacheRefreshTime;
				}
			}
		}

		public void FixedUpdate()
		{
			if (MultiTool.Renderer.Show) return;

			try
			{
				_active?.FixedUpdate();
			}
			catch (Exception ex)
			{
				Logger.Log($"{_active.Name} - error during FixedUpdate(). Details: {ex}", Logger.LogLevel.Error, "ToolController");
				_active.IncrementErrors();
			}
		}

		/// <summary>
		/// Register a new tool.
		/// </summary>
		/// <param name="tool">Tool to register</param>
		/// <returns>Identifier of the added tool</returns>
		public string Register(Tool tool)
		{
			// Find caller mod name.
			Assembly caller = Assembly.GetCallingAssembly();
			Mod callerMod = ModLoader.LoadedMods.FirstOrDefault(m => m.GetType().Assembly.GetName().Name == caller.GetName().Name);

			tool.Source = callerMod.Name;
			tool.Id = tool.Name.ToLower().Replace(' ', '_');
			tool.Services = _services;
			tool.Tools = this;

			// Block duplicate tool registration.
			if (_tools.FirstOrDefault(t => t.Id == tool.Id) != null)
				return tool.Id;

			tool.OnRegister();

			_tools.Add(tool);
			if (tool.HasCache)
				_cacheTools.Add(tool);

			return tool.Id;
		}

		/// <summary>
		/// Set tool as active.
		/// </summary>
		/// <param name="id">Tool ID</param>
		public void Activate(string id)
		{
			Deactivate();
			_active = GetById(id);
			_active.OnActivate();
		}

		/// <summary>
		/// Deactivate current tool.
		/// </summary>
		public void Deactivate()
		{
			if (_active == null) return;
			_active.OnDeactivate();
			_active.SelectedObject = null;
			_active = null;
		}

		/// <summary>
		/// Toggle tool state.
		/// </summary>
		/// <param name="id">Tool ID</param>
		public void Toggle(string id)
		{
			if (IsActive(id))
				Deactivate();
			else
				Activate(id);
		}

		public bool IsActive(string id)
		{
			return _active?.Id == id;
		}

		/// <summary>
		/// Get a registered tool by ID.
		/// </summary>
		/// <param name="id">Tool ID</param>
		/// <returns>Tool if exists, otherwise null</returns>
		public Tool GetById(string id)
		{
			return _tools.FirstOrDefault(t => t.Id == id);
		}

		public string GetName(string id)
			=> GetById(id)?.Name;

		public string GetAccessibleName(string id)
			=> Accessibility.GetAccessibleString(GetName(id), IsActive(id));

		public void RenderHud()
		{
			if (_active == null || MultiTool.Renderer.Show) return;

			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
			try
			{
				if (_active.UsesDefaultObjectSelectionUI)
				{
					GUILayout.BeginVertical();
					GUILayout.Space(Screen.height * 0.05f);
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Button(
						$"Selected object: {(_active.SelectedObject != null ? _active.SelectedObject.name : "None")}\n{_services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action1)} to {(_active.SelectedObject != null ? "deselect" : "select")}",
						GUILayout.MinHeight(50f)
					);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
				_active.HudRender();
			}
			catch (Exception ex)
			{
				Logger.Log($"{_active.Name} - error during HudRender(). Details: {ex}", Logger.LogLevel.Error, "ToolController");
				_active.IncrementErrors();
			}
			GUILayout.EndArea();
		}

		public void RenderControl(string id)
		{
			var tool = GetById(id);
			if (tool == null) return;

			try
			{
				tool.ControlRender();
			}
			catch (Exception ex)
			{
				Logger.Log($"{tool.Name} - error during ControlRender(). Details: {ex}", Logger.LogLevel.Error, "ToolController");
				tool.IncrementErrors();
			}
		}
	}
}
