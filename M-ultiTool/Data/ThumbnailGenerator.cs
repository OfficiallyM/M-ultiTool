using MultiTool.Services;
using MultiTool.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TLDLoader;
using UnityEngine;

namespace MultiTool.Data
{
	internal static class ThumbnailGenerator
	{
		private static ServiceContext _services;
		private struct PendingThumbnail
		{
			public GameObject GameObject;
			public int? Variant;
			public bool POI;
			public Action<Texture2D> OnGenerated;
		}

		private class Runner : MonoBehaviour { }

		private static readonly Queue<PendingThumbnail> _pending = new Queue<PendingThumbnail>();
		private static Runner _runner;
		private static bool _isProcessing = false;
		private const int _perFrame = 2;
		private static string _cacheDir = null;

		public static void Bootstrap(ServiceContext services)
		{
			_services = services;

			string configDir = Path.Combine(ModLoader.ModsFolder, "Config", "Mod Settings", MultiTool.ModInstance.ID);
			DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(configDir, "Cache"));
			_cacheDir = dir.FullName;
		}

		/// <summary>
		/// Trigger a full cache rebuild.
		/// </summary>
		internal static void RebuildCache()
		{
			DirectoryInfo cacheDirectory = new DirectoryInfo(_cacheDir);
			foreach (FileInfo file in cacheDirectory.GetFiles())
				file.Delete();

			_services.Database.FetchData();
		}

		/// <summary>
		/// Retrieves or generates a thumbnail texture for the specified GameObject.
		/// </summary>
		/// <param name="item">The GameObject to generate a thumbnail for.</param>
		/// <param name="onGenerated">Callback invoked when the thumbnail generation completes, receiving the generated Texture2D.</param>
		/// <param name="variant">Optional variant identifier used to differentiate cached thumbnails for the same item. If null, the cache key uses only the item name.</param>
		/// <param name="POI">If true, indicates this is a Point of Interest thumbnail; otherwise, false.</param>
		/// <returns>
		/// A Texture2D containing the cached thumbnail if available; otherwise, null.
		/// The actual generated texture is provided asynchronously via the <paramref name="onGenerated"/> callback.
		/// </returns>
		public static Texture2D GetThumbnail(GameObject item, Action<Texture2D> onGenerated, int? variant = null, bool POI = false)
		{
			string path = Path.Combine(_cacheDir, CacheFileName(item.name, variant));
			if (File.Exists(path))
			{
				Texture2D texture2D = new Texture2D(200, 200);
				ImageConversion.LoadImage(texture2D, File.ReadAllBytes(path));
				texture2D.Apply();
				return texture2D;
			}

			_pending.Enqueue(new PendingThumbnail { GameObject = item, Variant = variant, POI = POI, OnGenerated = onGenerated });
			EnsureProcessing();
			return null;
		}

		private static void EnsureProcessing()
		{
			if (_isProcessing) return;
			_isProcessing = true;

			if (_runner == null)
			{
				GameObject runnerObject = new GameObject("ThumbnailGenerator Runner");
				UnityEngine.Object.DontDestroyOnLoad(runnerObject);
				_runner = runnerObject.AddComponent<Runner>();
			}

			_runner.StartCoroutine(ProcessQueue());
		}

		private static Texture2D GenerateThumbnail(GameObject item, int? variant = null, bool POI = false)
		{
			GameObject gameObject = new GameObject("THUMBNAIL GENERATOR FOR " + item.name.ToUpper());
			gameObject.transform.position = new Vector3(UnityEngine.Random.Range(-200f, 200f), UnityEngine.Random.Range(-1000f, -9999f), UnityEngine.Random.Range(-200f, 200f));
			gameObject.layer = 1;
			gameObject.SetActive(false);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(item, gameObject.transform, false);

			// Change model variant.
			if (variant != null)
			{
				randomTypeSelector component = item.GetComponent<randomTypeSelector>();
				if (component != null)
				{
					component.rtipus = variant.Value;
					component.started = true;
					component.Refresh();
				}
			}

			// Render all thumbnails in pristine and in white.
			try
			{
				partconditionscript condition = gameObject2.GetComponent<partconditionscript>();
				if (condition != null)
					GameUtilities.SetConditionAndPaint(0, Color.white, condition);
			}
			catch { }

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
			if (obj == null)
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
			File.WriteAllBytes(Path.Combine(_cacheDir, CacheFileName(item.name, variant)), texture2D.EncodeToPNG());

			return texture2D;
		}

		private static IEnumerator ProcessQueue()
		{
			int total = _pending.Count;
			try
			{
				while (_pending.Count > 0)
				{
					for (int i = 0; i < _perFrame && _pending.Count > 0; i++)
					{
						PendingThumbnail next = _pending.Dequeue();
						Texture2D texture = null;
						try
						{
							texture = GenerateThumbnail(next.GameObject, next.Variant, next.POI);
						}
						catch (Exception ex)
						{
							Services.Logger.Log($"Thumbnail generation failed for {next.GameObject?.name ?? "Unknown"} - {ex}", Services.Logger.LogLevel.Error);
						}

						try
						{
							next.OnGenerated?.Invoke(texture);
						}
						catch (Exception ex)
						{
							Services.Logger.Log($"Thumbnail callback failed for {next.GameObject?.name ?? "Unknown"} - {ex}", Services.Logger.LogLevel.Error);
						}
					}
					yield return null;
				}
			}
			finally
			{
				Services.Logger.Log($"Thumbnail generation complete ({total} generated)");
				_isProcessing = false;
			}
		}

		private static string FormatName(string name)
		{
			return name.ToUpper().Replace("/", "or");
		}

		private static string CacheFileName(string name, int? variant = null)
		{
			string fileName = FormatName(name);
			if (variant != null)
				fileName += $"-{variant.Value - 1}";
			fileName += ".png";
			return fileName;
		}
	}
}
