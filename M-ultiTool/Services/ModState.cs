namespace MultiTool.Services
{
	/// <summary>
	/// Shared mutable mod state. A single instance is created at bootstrap
	/// (see MultiTool.cs) and handed out via ServiceContext - nothing should
	/// construct this directly.
	/// </summary>
	internal class ModState
	{
		public bool GodMode { get; set; } = false;
		public bool SpawnWithFuel { get; set; } = true;
		public string Mode { get; set; } = null;
		public carscript Car { get; set; } = null;
		public string SlotStage { get; set; } = null;
		public bool ShowColliders { get; set; } = false;
		public bool ShowColliderHelp { get; set; } = false;
	}
}
