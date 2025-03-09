using UnityEngine;

namespace MultiTool.Core
{
	internal class Notification
	{
		public enum NotificationType
		{
			Success,
			Warning,
			Error,
			Information,
		}

		public enum NotificationLength
		{
			VeryShort = 3,
			Short = 5,
			Medium = 7,
			Long = 10,
		}

		public string Title { get; set; }
		public string Message { get; set; }
		public NotificationType Type { get; set; }
		public NotificationLength Length { get; set; } = NotificationLength.Short;
		public float StartTime { get; set; }
		public Rect LastRenderRect { get; set; }
	}
}
