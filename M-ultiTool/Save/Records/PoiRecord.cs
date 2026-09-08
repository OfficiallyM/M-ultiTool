namespace MultiTool.Save.Records
{
	internal class PoiRecord : SaveRecord
	{
		public string Poi { get; set; }

		public PoiRecord()
		{
			RequiresInstantiation = true;
		}
	}
}
