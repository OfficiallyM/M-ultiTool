using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLDLoader;

namespace MultiTool.Modules
{
	public static class Logger
	{
		private static string logFile = "";
		private static bool initialised = false;
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
			if (!initialised) 
			{
				// Create logs directory.
				if (Directory.Exists(ModLoader.ModsFolder))
				{
					Directory.CreateDirectory(Path.Combine(ModLoader.ModsFolder, "Logs"));
					logFile = ModLoader.ModsFolder + "\\Logs\\M-ultiTool.log";
					File.WriteAllText(logFile, $"M-ultiTool v{Meta.Version} initialised\r\n");
					initialised = true;
				}
			}
		}

		/// <summary>
		/// Log messages to a file.
		/// </summary>
		/// <param name="msg">The message to log</param>
		public static void Log(string msg, LogLevel logLevel = LogLevel.Info)
		{
			if (logFile != string.Empty)
				File.AppendAllText(logFile, $"[{logLevel}] {msg}\r\n");
		}
	}
}
