using MultiTool.Config;
using MultiTool.Data;

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
		public Database Database { get; }
		public Translator Translator { get; }

		public ServiceContext()
		{
			Configuration = new Configuration();
			Keybinds = new Keybinds();
			State = new ModState();
			Translator = new Translator();
			Database = new Database(this);
		}
	}
}
