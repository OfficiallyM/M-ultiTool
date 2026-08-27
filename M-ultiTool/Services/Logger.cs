using System;
using System.IO;
using TLDLoader;

namespace MultiTool.Services
{
	internal static class Logger
	{
		private static string _logFile = "";
		private static bool _initialised = false;
		public enum LogLevel
		{
			Debug,
			Info,
			Warning,
			Error,
			Critical
		}

		public static void Init()
		{
			if (!_initialised)
			{
				// Create logs directory.
				if (Directory.Exists(ModLoader.ModsFolder))
				{
					Directory.CreateDirectory(Path.Combine(ModLoader.ModsFolder, "Logs"));
					_logFile = ModLoader.ModsFolder + "\\Logs\\M-ultiTool.log";
					File.WriteAllText(_logFile, $"M-ultiTool v{MultiTool.ModInstance.Version} initialised\r\n");
					_initialised = true;
				}
			}
		}

		/// <summary>
		/// Log messages to a file.
		/// </summary>
		/// <param name="msg">The message to log</param>
		public static void Log(string msg, LogLevel logLevel = LogLevel.Info)
		{
			if (_logFile != string.Empty)
				File.AppendAllText(_logFile, $"{DateTime.Now.ToString("s")} [{logLevel}] {msg}\r\n");
		}
	}
}
