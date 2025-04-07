using MultiTool.Core;
using MultiTool.Modules;
using MultiTool.Utilities;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using UnityEngine;

namespace MultiTool.Tabs
{
	internal class DebugTab : Tab
	{
		public override string Name => "Mod debug";
        public override bool ShowInNavigation => false;
        internal override bool IsFullScreen => true;
		private Vector2 _position;
		private string _data;

		public override void RenderTab(Rect dimensions)
		{
			if (string.IsNullOrEmpty(_data))
			{
				_data = PrettifyJson<Save>(SaveUtilities.GetRawSaveData());
			}

			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Back", GUILayout.MaxWidth(200)))
			{
				GUIRenderer.Tabs.ToggleActive(null);
			}
			GUILayout.Space(5);

			if (GUILayout.Button("Refresh", GUILayout.MaxWidth(200)))
			{
				_data = PrettifyJson<Save>(SaveUtilities.GetRawSaveData());
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);

			_position = GUILayout.BeginScrollView(_position);
			_data = GUILayout.TextArea(_data);
			GUILayout.EndScrollView();

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private static string PrettifyJson<T>(string json)
		{
			var serializer = new DataContractJsonSerializer(typeof(T));
			MemoryStream msRead = null;
			MemoryStream msWrite = null;
			XmlDictionaryWriter writer = null;

			try
			{
				msRead = new MemoryStream(Encoding.UTF8.GetBytes(json));
				T obj = (T)serializer.ReadObject(msRead);

				msWrite = new MemoryStream();
				writer = JsonReaderWriterFactory.CreateJsonWriter(msWrite, Encoding.UTF8, ownsStream: false, indent: true, indentChars: "  ");
				serializer.WriteObject(writer, obj);
				writer.Flush();

				return Encoding.UTF8.GetString(msWrite.ToArray());
			}
			finally
			{
				if (writer != null) writer.Close();
				if (msRead != null) msRead.Dispose();
				if (msWrite != null) msWrite.Dispose();
			}
		}
	}
}
