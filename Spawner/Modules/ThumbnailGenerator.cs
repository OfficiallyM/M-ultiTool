using System;
using System.Collections.Generic;
using System.IO;
using TLDLoader;
using UnityEngine;

namespace SpawnerTLD.Modules
{
	internal class ThumbnailGenerator
	{
		private string cacheDir = "";
		private bool regenerateCache = false;
		private Mod mod;
		private Logger logger;

		public ThumbnailGenerator(Mod _mod)
		{
			mod = _mod;

			logger = new Logger();

			// Create cache directory.
			if (Directory.Exists(ModLoader.GetModConfigFolder(mod)))
			{
				DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(ModLoader.GetModConfigFolder(mod), "Cache"));
				cacheDir = dir.FullName;
			}
		}

		/// <summary>
		/// Prepare cache for thumbnail generation.
		/// </summary>
		public void PrepareCache()
		{
			List<string> tempItems = new List<string>();

			// Remove vehicles and trailers from items count.
			foreach (GameObject item in itemdatabase.d.items)
			{
				if (!Utility.IsVehicleOrTrailer(item) && item.name != "ErrorPrefab" && !tempItems.Contains(item.name.ToUpper()))
				{
					tempItems.Add(item.name.ToUpper());
				}
			}

			int itemCount = tempItems.Count;
			DirectoryInfo cache = new DirectoryInfo(cacheDir);
			int cacheCount = cache.GetFiles().Length;

			// Item count differs, remove all cached thumbnails.
			if (itemCount != cacheCount)
			{
				regenerateCache = true;
				logger.Log("Item count has changed, regenerating cached thumbnails", Logger.LogLevel.Info);
				foreach (FileInfo file in cache.GetFiles())
				{
					file.Delete();
				}
			}
		}

		/// <summary>
		/// Load thumbnail from cache or generate if it doesn't exist
		/// </summary>
		/// <param name="item">Item to generate the thumbnail for</param>
		/// <returns>Texture2D thumbnail of the item</returns>
		public Texture2D GetThumbnail(GameObject item)
		{
			if (!regenerateCache && File.Exists(Path.Combine(cacheDir, item.name.ToUpper() + ".png")))
			{
				RenderTexture renderTexture = new RenderTexture(100, 100, 16);
				Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
				byte[] cacheImage = File.ReadAllBytes(Path.Combine(cacheDir, item.name.ToUpper() + ".png"));
				ImageConversion.LoadImage(texture2D, cacheImage);
				texture2D.Apply();
				return texture2D;
			}

			return GenerateThumbnail(item);
		}

		/// <summary>
		/// Item thumbnail generator
		/// </summary>
		/// <param name="item">The item to generate a thumbnail for</param>
		/// <returns>Texture2D thumbnail of the item</returns>
		private Texture2D GenerateThumbnail(GameObject item)
		{
			GameObject gameObject = new GameObject("THUMBNAIL GENERATOR FOR " + item.name.ToUpper());
			gameObject.transform.position = new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-200f, -100f), UnityEngine.Random.Range(-100f, 100f));
			gameObject.layer = 1;
			gameObject.SetActive(false);
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(item, gameObject.transform, false);
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
			camera.transform.LookAt((num >= ((Bounds)obj).size.magnitude + 1f) ? gameObject2.transform.position : ((Bounds)obj).center);
			num = Mathf.Max(((Bounds)obj).size.magnitude, num * 1.5f);
			camera.farClipPlane = Mathf.Max(10f, num * 1.5f);
			camera.nearClipPlane = 0.0001f;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = Color.clear;
			camera.orthographic = true;
			camera.orthographicSize = num / 3f;
			camera.gameObject.AddComponent<Light>().type = LightType.Directional;
			RenderTexture renderTexture = new RenderTexture(100, 100, 16);
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
			File.WriteAllBytes(Path.Combine(cacheDir, item.name.ToUpper() + ".png"), texture2D.EncodeToPNG());

			return texture2D;
		}
	}
}
