using SpawnerTLD.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLDLoader;

namespace SpawnerTLD.Modules
{
	internal class Logger
	{
		private readonly string logFile = "";
		public enum LogLevel
		{
			Debug,
			Info,
			Warning,
			Error,
			Critical
		}

		public Logger()
		{
			// Create logs directory.
			if (Directory.Exists(ModLoader.ModsFolder))
			{
				Directory.CreateDirectory(Path.Combine(ModLoader.ModsFolder, "Logs"));
				logFile = ModLoader.ModsFolder + "\\Logs\\SpawnerTLD.log";
				File.WriteAllText(logFile, $"SpawnerTLD v{Meta.Version} initialised\r\n");
			}
		}

		/// <summary>
		/// Log messages to a file.
		/// </summary>
		/// <param name="msg">The message to log</param>
		public void Log(string msg, LogLevel logLevel)
		{
			if (logFile != string.Empty)
				File.AppendAllText(logFile, $"[{logLevel}] {msg}\r\n");
		}
	}
}
