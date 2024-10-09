using MultiTool.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TLDLoader;

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
		public void AddTab(Tab tab)
        {
            // Find caller mod name.
            Assembly caller = Assembly.GetCallingAssembly();
            Mod callerMod = ModLoader.LoadedMods.Where(m => m.GetType().Assembly.GetName().Name == caller.GetName().Name).FirstOrDefault();

            if (ModLoader.isOnMainMenu)
            {
                Logger.Log($"Mod {callerMod.Name} attempted to register a tab too early. Tabs should be registered during OnLoad().", Logger.LogLevel.Error);
                return;
            }

            tab.Source = callerMod.Name;
            tab.Id = tab.Name.ToLower().Replace(' ', '_');

            Logger.Log($"Registered tab {tab.Name} (ID: {tab.Id}) via {tab.Source}");

            tab.OnRegister();

            _tabs.Add(tab);
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
    }
}
