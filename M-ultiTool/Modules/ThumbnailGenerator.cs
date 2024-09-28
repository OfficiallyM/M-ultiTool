using MultiTool.Core;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using TLDLoader;
using UnityEngine;

namespace MultiTool.Modules
{
	internal static class ThumbnailGenerator
	{
		private static string cacheDir = null;
		private static bool regenerateCache = false;

		public static void Init()
		{
			string configDir =  Path.Combine(ModLoader.ModsFolder, "Config", "Mod Settings", MultiTool.mod.ID);
			DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(configDir, "Cache"));
			cacheDir = dir.FullName;
		}

		/// <summary>
		/// Format name to cache format.
		/// </summary>
		/// <param name="name">Name to format</param>
		/// <returns>Formatted name</returns>
		private static string FormatName(string name)
		{
			return name.ToUpper().Replace("/", "or");
		}

		/// <summary>
		/// Trigger a full cache rebuild.
		/// </summary>
		internal static void RebuildCache()
		{
			DirectoryInfo cacheDirectory = new DirectoryInfo(cacheDir);
			foreach (FileInfo file in cacheDirectory.GetFiles())
			{
				file.Delete();
			}

			DatabaseUtilities.RebuildCaches();

			Logger.Log($"Successfully rebuilt thumbnail cache ({cacheDirectory.GetFiles().Length} thumbnails cached)");
		}

		/// <summary>
		/// Load thumbnail from cache or generate if it doesn't exist
		/// </summary>
		/// <param name="item">Item to generate the thumbnail for</param>
		/// <param name="variant">Optional variant index for the item</param>
		/// <returns>Texture2D thumbnail of the item</returns>
		public static Texture2D GetThumbnail(GameObject item, int? variant = null, bool POI = false)
		{
			string fileName = FormatName(item.name);
			if (variant != null)
			{
				fileName += $"-{variant.Value - 1}";
			}
			fileName += ".png";
			if (!regenerateCache && File.Exists(Path.Combine(cacheDir, fileName)))
			{
				RenderTexture renderTexture = new RenderTexture(200, 200, 16);
				Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
				byte[] cacheImage = File.ReadAllBytes(Path.Combine(cacheDir, fileName));
				ImageConversion.LoadImage(texture2D, cacheImage);
				texture2D.Apply();
				return texture2D;
			}

			return GenerateThumbnail(item, variant, POI);
		}

		/// <summary>
		/// Item thumbnail generator
		/// </summary>
		/// <param name="item">The item to generate a thumbnail for</param>
		/// <param name="variant">Optional variant index for the item</param>
		/// <returns>Texture2D thumbnail of the item</returns>
		private static Texture2D GenerateThumbnail(GameObject item, int? variant = null, bool POI = false)
		{
			GameObject gameObject = new GameObject("THUMBNAIL GENERATOR FOR " + item.name.ToUpper());
			gameObject.transform.position = new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-200f, -100f), UnityEngine.Random.Range(-100f, 100f));
			gameObject.layer = 1;
			gameObject.SetActive(false);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(item, gameObject.transform, false);

			// Change model variant.
			if (variant != null)
			{
				randomTypeSelector component = item.GetComponent<randomTypeSelector>();
				if (component != null)
				{
					component.forceStart = false;
					component.rtipus = variant.Value;
					component.Refresh();
				}
			}

			// Render all thumbnails in pristine and in white.
			partconditionscript condition = gameObject2.GetComponent<partconditionscript>();
			if (condition != null)
				GameUtilities.SetConditionAndPaint(0, Color.white, condition);

			gameObject2.transform.SetParent(gameObject.transform, false);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.layer = gameObject.layer;

			object obj = null;
			float num = 0.001f;

			try
			{
				Material material = null;
				foreach (Renderer renderer in gameObject2.GetComponentsInChildren<Renderer>(true))
				{
					try
					{
						if (renderer.gameObject.layer == 18)
						{
							renderer.gameObject.SetActive(renderer.enabled = false);
						}
						if (renderer.material == null && material != null)
						{
							renderer.material = material;
						}
						else
						{
							material = renderer.material;
						}
						renderer.gameObject.layer = gameObject.layer;
						if (obj == null)
						{
							obj = new Bounds(renderer.bounds.center, renderer.bounds.size);
						}
						else
						{
							((Bounds)obj).Encapsulate(renderer.bounds);
						}
						num = Mathf.Max(num, renderer.bounds.size.magnitude);
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			try
			{
				foreach (MonoBehaviour monoBehaviour in gameObject2.GetComponentsInChildren<MonoBehaviour>(true))
				{
					if (Array.IndexOf(new Type[]
					{
						typeof(Transform),
						typeof(Renderer),
						typeof(MeshRenderer),
						typeof(SkinnedMeshRenderer),
						typeof(MeshFilter)
					}, monoBehaviour.GetType()) == -1)
					{
						monoBehaviour.enabled = false;
					}
					UnityEngine.Object.Destroy(monoBehaviour.gameObject);
				}
			}
			catch
			{
			}
			Camera camera = new GameObject("CAMERA").AddComponent<Camera>();
			camera.gameObject.layer = gameObject.layer;
			camera.transform.SetParent(gameObject.transform, false);
			camera.transform.localPosition = new Vector3(1f, 1f, 1f) * num;
			if (POI && obj == null)
			{
				camera.transform.LookAt(gameObject2.transform.position);
				num = 1f;
			}
			else
			{
				camera.transform.LookAt((num >= ((Bounds)obj).size.magnitude + 1f) ? gameObject2.transform.position : ((Bounds)obj).center);
				num = Mathf.Max(((Bounds)obj).size.magnitude, num * 1.5f);
			}
			camera.farClipPlane = Mathf.Max(10f, num * 1.5f);
			camera.nearClipPlane = 0.0001f;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = Color.clear;
			camera.orthographic = true;
			camera.orthographicSize = num / 3f;
			camera.gameObject.AddComponent<Light>().type = LightType.Directional;
			RenderTexture renderTexture = new RenderTexture(200, 200, 16);
			camera.forceIntoRenderTexture = true;
			camera.targetTexture = renderTexture;
			gameObject.SetActive(true);
			camera.Render();
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = renderTexture;
			Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
			texture2D.ReadPixels(new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			gameObject.SetActive(false);
			gameObject2.SetActive(false);
			UnityEngine.Object.Destroy(renderTexture);
			UnityEngine.Object.Destroy(gameObject);
			UnityEngine.Object.Destroy(gameObject2);

			// Write texture to cache.
			string fileName = item.name.ToUpper().Replace("/", "or");
			if (variant != null)
			{
				fileName += $"-{variant.Value - 1}";
			}
			fileName += ".png";
			File.WriteAllBytes(Path.Combine(cacheDir, fileName), texture2D.EncodeToPNG());

			return texture2D;
		}
	}
}
