namespace MultiTool.Services
{
    /// <summary>
    /// Shared mutable mod state. A single instance is created at bootstrap
    /// (see MultiTool.cs) and handed out via ServiceContext - nothing should
    /// construct this directly.
    /// </summary>
    internal class ModState
    {
        public bool DeleteMode { get; set; } = false;
        public bool GodMode { get; set; } = false;
        public bool Noclip { get; set; } = false;
        public bool SpawnWithFuel { get; set; } = true;
        public string Mode { get; set; } = null;
        public carscript Car { get; set; } = null;
        public string SlotStage { get; set; } = null;
        public bool ShowCoords { get; set; } = false;
        public bool ObjectDebug { get; set; } = false;
        public bool AdvancedObjectDebug { get; set; } = false;
        public bool ObjectDebugShowUnity { get; set; } = true;
        public bool ObjectDebugShowCore { get; set; } = true;
        public bool ObjectDebugShowChildren { get; set; } = true;
        public bool ShowColliders { get; set; } = false;
        public bool ShowColliderHelp { get; set; } = false;
    }
}
