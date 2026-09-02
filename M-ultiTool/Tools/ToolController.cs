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
		private List<Tool> _backgroundActive = new List<Tool>();
		private List<Tool> _tools = new List<Tool>();
		private List<Tool> _cacheTools = new List<Tool>();
		private List<Tool> _processingTools = new List<Tool>();
		private bool _processObjectSelection = false;

		public ToolController(ServiceContext services)
		{
			_services = services;
		}

		public void Update()
		{
			if (MultiTool.Renderer.Show) return;

			tosaveitemscript selectedObject = null;
			if (_processObjectSelection)
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
							selectedObject = save;
						else
							selectedObject = null;
					}
					else
					{
						selectedObject = null;
					}
				}
			}

			foreach (var tool in _processingTools)
			{
				if (tool.IsDisabled) continue;
				if (tool.UsesObjectSelection)
					tool.SelectedObject = selectedObject;

				try
				{
					tool.Update();
				}
				catch (Exception ex)
				{
					Logger.Log($"{tool.Name} - error during Update(). Details: {ex}", Logger.LogLevel.Error, "ToolController");
					tool.IncrementErrors();
				}
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

			foreach (var tool in _processingTools)
			{
				if (tool.IsDisabled) continue;

				try
				{
					tool.FixedUpdate();
				}
				catch (Exception ex)
				{
					Logger.Log($"{tool.Name} - error during FixedUpdate(). Details: {ex}", Logger.LogLevel.Error, "ToolController");
					tool.IncrementErrors();
				}
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
			tool.Id = tool.Name.ToLowerInvariant().Replace(' ', '_');
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
			var tool = GetById(id);
			if (tool == null || tool.IsDisabled) return;
			if (tool.IsExclusive)
			{
				Deactivate(id);
				_active = tool;
			}
			else
			{
				_backgroundActive.Add(tool);
			}
			tool.OnActivate();
			BuildProcessingTools();
		}

		/// <summary>
		/// Deactivate current tool.
		/// </summary>
		public void Deactivate(string id)
		{
			var tool = GetById(id);
			if (tool == null) return;
			if (tool.IsExclusive)
			{
				_active.OnDeactivate();
				_active.SelectedObject = null;
				_active = null;
			}
			else
			{
				_backgroundActive.Remove(tool);
			}
			BuildProcessingTools();
		}

		/// <summary>
		/// Toggle tool state.
		/// </summary>
		/// <param name="id">Tool ID</param>
		public void Toggle(string id)
		{
			if (IsActive(id))
				Deactivate(id);
			else
				Activate(id);
		}

		public bool IsActive(string id)
		{
			return _active?.Id == id || _backgroundActive.FirstOrDefault(t => t.Id == id) != null;
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
			if (_processingTools.Count <= 0 || MultiTool.Renderer.Show) return;

			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
			bool selectUiDrawn = false;
			foreach (var tool in _processingTools)
			{
				try
				{
					if (tool.UsesDefaultObjectSelectionUI && !selectUiDrawn)
					{
						GUILayout.BeginVertical();
						GUILayout.Space(Screen.height * 0.05f);
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						GUILayout.Button(
							$"Selected object: {(tool.SelectedObject != null ? tool.SelectedObject.name : "None")}\n{_services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action1)} to {(tool.SelectedObject != null ? "deselect" : "select")}",
							GUILayout.MinHeight(50f)
						);
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
						selectUiDrawn = true;
					}
					tool.HudRender();
				}
				catch (Exception ex)
				{
					Logger.Log($"{tool.Name} - error during HudRender(). Details: {ex}", Logger.LogLevel.Error, "ToolController");
					tool.IncrementErrors();
				}
			}
			GUILayout.EndArea();
		}

		public void RenderControl(string id)
		{
			var tool = GetById(id);
			if (tool == null || tool.IsDisabled) return;

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

		private void BuildProcessingTools()
		{
			_processObjectSelection = false;
			_processingTools = new List<Tool>();
			if (_active != null)
				_processingTools.Add(_active);
			_processingTools.AddRange(_backgroundActive);

			foreach (var tool in _processingTools)
			{
				if (tool.UsesObjectSelection)
					_processObjectSelection = true;
			}
		}
	}
}
