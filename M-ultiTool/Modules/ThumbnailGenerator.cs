using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using TLDLoader;
using UnityEngine;

namespace MultiTool.Modules
{
	internal class ThumbnailGenerator
	{
		private string configDirectory;

		private string cacheDir = "";
		private bool regenerateCache = false;

		public ThumbnailGenerator(string _configDirectory)
		{
			configDirectory = _configDirectory;

			// Create cache directory.
			if (Directory.Exists(configDirectory))
			{
				DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(configDirectory, "Cache"));
				cacheDir = dir.FullName;
			}
		}

		/// <summary>
		/// Prepare cache for thumbnail generation.
		/// </summary>
		public void PrepareCache()
		{
			List<string> tempItems = new List<string>();

			// Remove unnecessary items and duplicates from item list.
			foreach (GameObject item in itemdatabase.d.items)
			{
				if (item.name != "ErrorPrefab")
				{
					if (GameUtilities.IsVehicleOrTrailer(item))
					{
						// Get vehicle variants.
						randomTypeSelector randoms = item.GetComponent<randomTypeSelector>();
						if (randoms != null && randoms.tipusok.Length > 0)
						{
							int variants = randoms.tipusok.Length;

							for (int i = 0; i < variants; i++)
							{
								if (!tempItems.Contains($"{item.name.ToUpper()}-{i + 1}"))
									tempItems.Add($"{item.name.ToUpper()}-{i + 1}");
							}
							continue;
						}
					}
					if (!tempItems.Contains(item.name.ToUpper()))
						tempItems.Add(item.name.ToUpper());
				}
			}

			foreach (GameObject POI in itemdatabase.d.buildings)
			{
				if (POI.name != "ErrorPrefab" && POI.name != "Falu01")
				{
					if (!tempItems.Contains(POI.name.ToUpper()))
					{
						tempItems.Add(POI.name.ToUpper());
					}
				}
			}

			foreach (ObjClass objClass in mainscript.M.terrainGenerationSettings.objGeneration.objTypes)
			{
				if (!tempItems.Contains(objClass.prefab.name.ToUpper()))
				{
					tempItems.Add(objClass.prefab.name.ToUpper());
				}
			}

			foreach (ObjClass objClass in mainscript.M.terrainGenerationSettings.desertTowerGeneration.objTypes)
			{
				if (!tempItems.Contains(objClass.prefab.name.ToUpper()))
				{
					tempItems.Add(objClass.prefab.name.ToUpper());
				}
			}

			int itemCount = tempItems.Count;
			DirectoryInfo cache = new DirectoryInfo(cacheDir);
			int cacheCount = cache.GetFiles().Length;

			// Item count differs, remove all cached thumbnails.
			if (itemCount != cacheCount)
			{
				regenerateCache = true;
				Logger.Log("Item count has changed, regenerating cached thumbnails", Logger.LogLevel.Info);
				Logger.Log($"Item count: {itemCount} - Cache count: {cacheCount}", Logger.LogLevel.Info);

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
		/// <param name="variant">Optional variant index for the item</param>
		/// <returns>Texture2D thumbnail of the item</returns>
		public Texture2D GetThumbnail(GameObject item, int? variant = null, bool POI = false)
		{
			string fileName = item.name.ToUpper().Replace("/", "or");
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
		private Texture2D GenerateThumbnail(GameObject item, int? variant = null, bool POI = false)
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
			if (gameObject2.GetComponent<partconditionscript>() != null)
			{
				gameObject2.GetComponent<partconditionscript>().StartPaint(0, new Color(1f, 1f, 1f));
			}

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
