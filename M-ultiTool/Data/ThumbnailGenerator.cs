using MultiTool.Extensions;
using MultiTool.Services;
using MultiTool.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TLDLoader;
using UnityEngine;
using Logger = MultiTool.Services.Logger;

namespace MultiTool.Data
{
	internal static class ThumbnailGenerator
	{
		private static ServiceContext _services;

		public enum ThumbnailType
		{
			Item,
			Vehicle,
			Poi,
		} 

		private struct PendingThumbnail
		{
			public GameObject GameObject;
			public int? Variant;
			public ThumbnailType Type;
			public Action<Texture2D> OnGenerated;
		}

		private class Runner : MonoBehaviour { }

		private static readonly Queue<PendingThumbnail> _pending = new Queue<PendingThumbnail>();
		private static Runner _runner;
		private const int _perFrame = 10;
		private static string _cacheDir = null;

		public static bool IsProcessing = false;

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
		/// <param name="type">Specifies which item type the thumbnail is for.</param>
		/// <returns>
		/// A Texture2D containing the cached thumbnail if available; otherwise, null.
		/// The actual generated texture is provided asynchronously via the <paramref name="onGenerated"/> callback.
		/// </returns>
		public static Texture2D GetThumbnail(GameObject item, Action<Texture2D> onGenerated, int? variant = null, ThumbnailType type = ThumbnailType.Item)
		{
			string path = Path.Combine(_cacheDir, CacheFileName(item.name, variant));
			if (File.Exists(path))
			{
				Texture2D texture2D = new Texture2D(200, 200);
				ImageConversion.LoadImage(texture2D, File.ReadAllBytes(path));
				texture2D.Apply();
				return texture2D;
			}

			_pending.Enqueue(new PendingThumbnail { GameObject = item, Variant = variant, Type = type, OnGenerated = onGenerated });
			return null;
		}

		public static void TriggerProcessing()
		{
			if (IsProcessing) return;
			if (_pending.Count == 0) return;
			IsProcessing = true;

			if (_runner == null)
			{
				GameObject runnerObject = new GameObject("ThumbnailGenerator Runner");
				UnityEngine.Object.DontDestroyOnLoad(runnerObject);
				_runner = runnerObject.AddComponent<Runner>();
			}

			_runner.StartCoroutine(ProcessQueue());
		}

		private static Texture2D GenerateThumbnail(PendingThumbnail pending)
		{
			GameObject root = new GameObject("THUMBNAIL GENERATOR FOR " + pending.GameObject.name.ToUpper());
			root.transform.position = new Vector3(UnityEngine.Random.Range(-200f, 200f), UnityEngine.Random.Range(-200f, -1000f), UnityEngine.Random.Range(-200f, 200f));
			root.layer = 1;
			root.SetActive(false);
			GameObject instance = UnityEngine.Object.Instantiate(pending.GameObject, root.transform, false);
			string pendingName = pending.GameObject.name.ToLowerInvariant();
			instance.transform.localPosition = Vector3.zero;
			instance.layer = root.layer;

			// Change model variant.
			if (pending.Variant != null)
			{
				randomTypeSelector component = pending.GameObject.GetComponent<randomTypeSelector>();
				if (component != null)
				{
					component.rtipus = pending.Variant.Value;
					component.started = true;
					component.Refresh();
				}
			}

			// Render all thumbnails in pristine and in white.
			try
			{
				partconditionscript condition = instance.GetComponent<partconditionscript>();
				if (condition != null)
					GameUtilities.SetConditionAndPaint(0, Color.white, condition);
			}
			catch { }

			// Ensure the object doesn't spawn any other cameras to conflict
			// with ours.
			try
			{
				foreach (var existingCamera in instance.GetComponentsInChildren<Camera>())
				{
					UnityEngine.Object.Destroy(existingCamera);
				}
			}
			catch { }

			// Rotate left doors and gauges 180 degrees to face the camera.
			if (pendingName.Contains("ldoor") || pending.GameObject.GetComponent<meterscript>() != null)
			{
				Vector3 angle = instance.transform.localEulerAngles;
				angle.y = 180f;
				instance.transform.localEulerAngles = angle;
			}

			Bounds? bounds = null;
			try
			{
				Material material = null;
				foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
				{
					try
					{
						if (renderer.gameObject.layer == 18)
						{
							renderer.gameObject.SetActive(renderer.enabled = false);
							continue;
						}

						// Particle systems aren't body geometry and can report a bounds at
						// world origin so exclude them.
						if (renderer is ParticleSystemRenderer)
							continue;

						// Guard against any other renderer reporting a zero-size bounds.
						if (renderer.bounds.size == Vector3.zero)
							continue;

						if (pending.Type == ThumbnailType.Poi)
						{
							// Skip ropes as they produce unusual boundaries.
							if (renderer.name.ToLowerInvariant() == "rope")
								continue;

							// Hide white circles from below buildings.
							if (renderer.name.ToLowerInvariant().Contains("helppos"))
							{
								renderer.enabled = false;
								continue;
							}
						}
						else
						{
							// Hide weird collider renderers from some vehicles.
							if (GameUtilities.IsVehicleOrTrailer(pending.GameObject) && renderer.gameObject.GetComponent<Collider>() != null)
								renderer.enabled = false;
						}

						// Reject any renderers that are implausibly far away.
						float distanceFromRoot = Vector3.Distance(renderer.bounds.center, instance.transform.position);
						if (distanceFromRoot > 500f)
							continue;

						if (renderer.material == null && material != null)
							renderer.material = material;
						else
							material = renderer.material;
						renderer.gameObject.layer = root.layer;
						if (bounds == null)
						{
							bounds = renderer.bounds;
						}
						else
						{
							Bounds expanded = bounds.Value;
							expanded.Encapsulate(renderer.bounds);
							bounds = expanded;
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}

			Camera camera = new GameObject("CAMERA").AddComponent<Camera>();
			camera.gameObject.AddComponent<Light>().type = LightType.Directional;
			camera.gameObject.layer = root.layer;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = Color.clear;
			camera.fieldOfView = 30f;
			camera.aspect = 1f;
			camera.cullingMask = 1 << root.layer;

			var direction = new Vector3(1f, -0.3f, -1f).normalized;
			Vector3 lookTarget = bounds?.center ?? instance.transform.position;
			float extentMagnitude = Mathf.Max(bounds?.extents.magnitude ?? 1f, 0.1f);
			float distance = extentMagnitude / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

			camera.transform.position = lookTarget - direction * distance;
			camera.transform.LookAt(lookTarget);
			camera.nearClipPlane = 0.0001f;
			camera.farClipPlane = distance * 3f;
			camera.transform.SetParent(instance.transform, true);

			RenderTexture renderTexture = new RenderTexture(200, 200, 16);
			camera.forceIntoRenderTexture = true;
			camera.targetTexture = renderTexture;
			root.SetActive(true);
			camera.Render();
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = renderTexture;
			Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
			texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;

			root.SetActive(false);
			instance.SetActive(false);
			UnityEngine.Object.Destroy(renderTexture);
			UnityEngine.Object.Destroy(root);
			UnityEngine.Object.Destroy(instance);

			// Write texture to cache.
			File.WriteAllBytes(Path.Combine(_cacheDir, CacheFileName(pending.GameObject.name, pending.Variant)), texture2D.EncodeToPNG());

			return texture2D;
		}

		private static IEnumerator ProcessQueue()
		{
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
							texture = GenerateThumbnail(next);
							next.OnGenerated?.Invoke(texture);
						}
						catch (Exception ex)
						{
							Logger.Log($"Thumbnail generation failed for {next.GameObject?.name ?? "Unknown"} - {ex}", Logger.LogLevel.Error);
						}
					}
					yield return null;
				}
			}
			finally
			{
				IsProcessing = false;
			}
		}

		private static string CacheFileName(string name, int? variant = null)
		{
			name = name.Trim();
			if (variant != null)
				name += $"_{variant.Value}";
			name = name.ToKey();
			name += ".png";
			return name;
		}
	}
}
