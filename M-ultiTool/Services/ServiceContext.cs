using MultiTool.Config;

namespace MultiTool.Services
{
	/// <summary>
	/// Bundles the services constructed once at bootstrap (see MultiTool.cs).
	/// Passed down explicitly to whatever needs them.
	///
	/// Logger, Translator and ThumbnailGenerator are deliberately not here -
	/// they're stateless-enough static utilities and stay that way.
	/// </summary>
	internal class ServiceContext
	{
		public Configuration Configuration { get; }
		public Keybinds Keybinds { get; }
		public ModState State { get; }

		public ServiceContext(Configuration configuration, Keybinds keybinds, ModState state)
		{
			Configuration = configuration;
			Keybinds = keybinds;
			State = state;
		}
	}
}
