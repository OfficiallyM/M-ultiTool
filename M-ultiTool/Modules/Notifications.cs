using MultiTool.Core;
using ScottPlot.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MultiTool.Core.Notification;

namespace MultiTool.Modules
{
	internal static class Notifications
	{
		private static List<Notification> _notifications = new List<Notification>();

		/// <summary>
		/// Send a notification.
		/// </summary>
		/// <param name="notification">Full notification object</param>
		public static void Send(Notification notification)
		{
			notification.StartTime = Time.unscaledTime;

			// Allow messages to be 128 characters minus the elipsis.
			if (notification.Message.Length > 125)
				notification.Message = notification.Message.Substring(0, 125) + "...";

			_notifications.Add(notification);
		}

		/// <summary>
		/// Send a notification.
		/// </summary>
		/// <param name="title">Notification title</param>
		/// <param name="message">Message body</param>
		/// <param name="type">Notification type</param>
		/// <param name="length">Notification length</param>
		public static void Send(string title, string message, NotificationType type = NotificationType.Success, NotificationLength length = NotificationLength.Short)
		{
			Send(new Notification()
			{
				Title = title,
				Message = message, 
				Type = type,
				Length = length
			});
		}

		/// <summary>
		/// Send a success notification.
		/// </summary>
		/// <param name="title">Notification title</param>
		/// <param name="message">Message body</param>
		/// <param name="length">Notification length</param>
		public static void SendSuccess(string title, string message, NotificationLength length = NotificationLength.Short)
		{
			Send(new Notification()
			{
				Title = title,
				Message = message,
				Type = NotificationType.Success,
				Length = length
			});
		}

		/// <summary>
		/// Send a warning notification.
		/// </summary>
		/// <param name="title">Notification title</param>
		/// <param name="message">Message body</param>
		/// <param name="length">Notification length</param>
		public static void SendWarning(string title, string message, NotificationLength length = NotificationLength.Short)
		{
			Send(new Notification()
			{
				Title = title,
				Message = message,
				Type = NotificationType.Warning,
				Length = length
			});
		}

		/// <summary>
		/// Send an error notification.
		/// </summary>
		/// <param name="title">Notification title</param>
		/// <param name="message">Message body</param>
		/// <param name="length">Notification length</param>
		public static void SendError(string title, string message, NotificationLength length = NotificationLength.Short)
		{
			Send(new Notification()
			{
				Title = title,
				Message = message,
				Type = NotificationType.Error,
				Length = length
			});
		}

		/// <summary>
		/// Send an information notification.
		/// </summary>
		/// <param name="title">Notification title</param>
		/// <param name="message">Message body</param>
		/// <param name="length">Notification length</param>
		public static void SendInformation(string title, string message, NotificationLength length = NotificationLength.Short)
		{
			Send(new Notification()
			{
				Title = title,
				Message = message,
				Type = NotificationType.Information,
				Length = length
			});
		}

		/// <summary>
		/// Render notification display. Note: This method needs to be called from an OnGUI() method.
		/// </summary>
		public static void Render()
		{
			Rect dimensions = new Rect(10, 10, MultiTool.Renderer.resolutionX - 20, MultiTool.Renderer.resolutionY - 20);

			GUILayout.BeginArea(dimensions);
			GUILayout.BeginVertical();
			foreach (Notification notification in _notifications)
			{
				string borderStyle = "BoxGreen";
				switch (notification.Type)
				{
					case NotificationType.Warning:
						borderStyle = "BoxOrange";
						break;
					case NotificationType.Error:
						borderStyle = "BoxRed";
						break;
					case NotificationType.Information:
						borderStyle = "BoxBlue";
						break;
				}

				GUILayout.BeginVertical(borderStyle, GUILayout.MaxWidth(300), GUILayout.MaxHeight(100));
				// Create an invisible box to use as the rect for the background.
				GUILayout.Box(string.Empty, "ButtonTransparent", GUILayout.MaxWidth(300), GUILayout.MaxHeight(100));
				if (Event.current.type == EventType.Repaint)
					notification.LastRenderRect = GUILayoutUtility.GetLastRect();
				Rect boxRect = notification.LastRenderRect;
				boxRect.x += 2f;
				boxRect.y += 2f;
				boxRect.width -= 4f;
				boxRect.height -= 4f;
				GUI.Box(boxRect, string.Empty, "BoxGrey");
				GUI.Label(new Rect(boxRect.x + 5f, boxRect.y + 5f, boxRect.width - 10f, 20), $"<b>{notification.Title}</b>", "LabelCenter");
				GUI.Label(new Rect(boxRect.x + 5f, boxRect.y + 30f, boxRect.width - 10f, boxRect.height - 30f), notification.Message, "LabelCenter");
				GUILayout.EndVertical();

				GUILayout.Space(5);
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		public static void Update()
		{
			if (_notifications.Count == 0) return;

			foreach (Notification notification in _notifications)
			{
				float currentTime = Time.unscaledTime;
				int diff = Mathf.RoundToInt(currentTime - notification.StartTime);

				if (diff >= (int)notification.Length)
				{
					_notifications.Remove(notification);
					break;
				}
			}
		}
	}
}
