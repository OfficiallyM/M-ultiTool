using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TLDLoader;
using UnityEngine;

namespace MultiTool.Modules
{
    public class TabController
    {
        private int _tab = 0;

        private List<Tab> _tabs = new List<Tab>();

        internal void Update()
        {
            foreach (Tab tab in _tabs)
                tab.Update();
        }

        /// <summary>
		/// Add new tab.
		/// </summary>
		/// <param name="tab"></param>
        /// <returns>Index of the added tab or -1 if tab is invalid</returns>
		public int AddTab(Tab tab)
        {
            // Find caller mod name.
            Assembly caller = Assembly.GetCallingAssembly();
            Mod callerMod = ModLoader.LoadedMods.Where(m => m.GetType().Assembly.GetName().Name == caller.GetName().Name).FirstOrDefault();

            if (ModLoader.isOnMainMenu)
            {
                Logger.Log($"Mod {callerMod.Name} attempted to register a tab too early. Tabs should be registered during OnLoad().", Logger.LogLevel.Error);
                return -1;
            }

            tab.Source = callerMod.Name;
            tab.Id = tab.Name.ToLower().Replace(' ', '_');

            // Block duplicate tab registration.
            if (_tabs.Where(t => t.Id == tab.Id).FirstOrDefault() != null)
                return -1;

            tab.OnRegister();

            _tabs.Add(tab);

            return _tabs.Count - 1;
        }

        /// <summary>
        /// Unregister all tabs to ensure they can be registered again next load.
        /// </summary>
        internal void UnregisterAll()
        {
            foreach (var tab in _tabs)
            {
                tab.OnUnregister();
            }

            _tabs.Clear();
        }

        /// <summary>
        /// Set the active tab.
        /// </summary>
        /// <param name="id">Id of the tab to set</param>
        /// <exception cref="KeyNotFoundException">Thrown if the tab index doesn't exist</exception>
        internal void SetActive(int id)
        {
            if (_tabs.Count > id && _tabs[id] != null)
                _tab = id;
            else
                throw new KeyNotFoundException();
        }

        /// <summary>
        /// Get the currently active tab index.
        /// </summary>
        /// <returns>Index of the currently active tab</returns>
        internal int GetActive() => _tab;

        /// <summary>
        /// Get number of tabs.
        /// </summary>
        /// <returns>Total number of tabs including disabled tabs</returns>
        internal int GetCount() => _tabs.Count;

        /// <summary>
        /// Get tab by index.
        /// </summary>
        /// <param name="id">Index of tab to find</param>
        /// <returns>Tab if the ID is valid, otherwise null</returns>
        internal Tab GetByIndex(int id) => _tabs[id];

        /// <summary>
		/// Render a given tab
		/// </summary>
		/// <param name="tab">The tab index to render</param>
		internal void RenderTab(int? tabIndex = null)
        {
            if (tabIndex == null) tabIndex = _tab;
            Tab tab = GetByIndex(tabIndex.Value);

            Rect tabDimensions = new Rect()
            {
                x = MultiTool.Renderer.mainMenuX - 30f,
                y = MultiTool.Renderer.mainMenuX + (tab.IsFullScreen ? 20f : 60f),
                width = MultiTool.Renderer.mainMenuWidth - 20f,
                height = MultiTool.Renderer.mainMenuHeight - (tab.IsFullScreen ? 70f : 110f),
            };

            float configWidth = tabDimensions.width * 0.25f;
            float configX = tabDimensions.x + tabDimensions.width - configWidth;

            // Return early if tab is disabled.
            if (tab.IsDisabled) return;

            // Config pane.
            if (tab.HasConfigPane)
            {
                // Decrease tab width to account for content pane.
                tabDimensions.width -= configWidth + 5f;

                GUI.Box(new Rect(configX, tabDimensions.y, configWidth, tabDimensions.height), "<size=16>Configuration</size>");

                Rect configDimensions = new Rect()
                {
                    x = configX,
                    y = tabDimensions.y + 20f,
                    width = configWidth - 10f,
                    height = tabDimensions.height - 20f,
                };

                try
                {
                    tab.RenderConfigPane(configDimensions);
                }
                catch (Exception ex)
                {
                    tab.Errors++;
                    Logger.Log($"Error occurred during tab \"{tab.Name}\" MultiTool.Configuration render ({tab.Errors}/5). Details: {ex}", Logger.LogLevel.Error);

                    if (tab.Errors >= 5)
                    {
                        tab.IsDisabled = true;
                        Logger.Log($"Tab {tab.Name} threw too many errors and has been disabled.", Logger.LogLevel.Warning);
                    }
                }
            }

            //GUILayout.BeginArea(new Rect(tabDimensions.x, tabDimensions.y, tabDimensions.width, tabDimensions.height), string.Empty, "box");
            try
            {
                tab.RenderTab(tabDimensions);
            }
            catch (Exception ex)
            {
                tab.Errors++;
                Logger.Log($"Error occurred during tab \"{tab.Name}\" render ({tab.Errors}/5). Details: {ex}", Logger.LogLevel.Error);

                if (tab.Errors >= 5)
                {
                    tab.IsDisabled = true;
                    Logger.Log($"Tab {tab.Name} threw too many errors and has been disabled.", Logger.LogLevel.Warning);
                }
            }
            //GUILayout.EndArea();
        }
    }
}
