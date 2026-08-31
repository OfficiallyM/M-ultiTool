using MultiTool.Services;
using MultiTool.UI;
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

		private Tool _active = null;
		private List<Tool> _tools = new List<Tool>();
		private List<Tool> _cacheTools = new List<Tool>();

		public ToolController(ServiceContext services)
		{
			_services = services;
		}

		public void Update()
		{
			_active?.Update();

			foreach (Tool tool in _cacheTools)
			{
				if (!tool.HasCache) continue;
				tool.NextCacheUpdate -= Time.unscaledDeltaTime;
				if (tool.NextCacheUpdate <= 0)
				{
					tool.OnCacheRefresh();
					tool.NextCacheUpdate = tool.CacheRefreshTime;
				}
			}
		}

		public void FixedUpdate()
		{
			_active?.FixedUpdate();
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
			_active?.OnDeactivate();
			_active = GetById(id);
			_active.OnActivate();
		}

		/// <summary>
		/// Deactivate current tool.
		/// </summary>
		public void Deactivate()
		{
			_active?.OnDeactivate();
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
			if (_active == null) return;

			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
			_active.HudRender();
			GUILayout.EndArea();
		}

		public void RenderControl(string id)
		{
			var tool = GetById(id);
			if (tool == null) return;

			tool.ControlRender();
		}
	}
}
