using MultiTool.Services;
using MultiTool.UI;
using MultiTool.Utilities;
using UnityEngine;

namespace MultiTool.Tools
{
	internal class ColourPickerTool : Tool
	{
		public override string Name => "Colour Picker";
		public override bool UsesObjectSelection => false;
		public override bool UsesDefaultObjectSelectionUI => false;

		private Texture2D _cachedPreview;

		public override void OnActivate()
		{
			CacheColourTexture();
			GUIRenderer.OnMenuToggle += OnMenuToggle;
		}

		public override void OnDeactivate()
		{
			GUIRenderer.OnMenuToggle -= OnMenuToggle;
		}

		public override void ControlRender()
		{
			string name = Name.ToLowerInvariant();
			if (GUILayout.Button(Accessibility.GetAccessibleString($"Toggle {name} mode", MultiTool.Tools.IsActive(Id)), GUILayout.MaxWidth(200)))
				MultiTool.Tools.Toggle(Id);
			GUILayout.Space(10);
		}

		public override void HudRender()
		{
			float fullWidth = Screen.width * 0.2f;
			float halfWidth = fullWidth / 2;

			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical("box", GUILayout.Width(fullWidth));
			GUILayout.BeginHorizontal();
			GUILayout.Button("Copy", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action1), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Button("Paste", GUILayout.Width(halfWidth));
			GUILayout.Button(Services.Keybinds.GetPrettyName((int)Keybinds.Inputs.action2), GUILayout.Width(halfWidth));
			GUILayout.EndHorizontal();

			// Colour preview.
			GUIStyle defaultStyle = GUI.skin.button;
			GUIStyle previewStyle = new GUIStyle(defaultStyle);
			previewStyle.normal.background = _cachedPreview;
			previewStyle.active.background = _cachedPreview;
			previewStyle.hover.background = _cachedPreview;
			GUI.skin.button = previewStyle;
			GUILayout.Button("");
			GUI.skin.button = defaultStyle;
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}

		public override void Update()
		{
			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action1).AssignedKey))
			{
				GameObject hitGameObject = Raycast();
				partconditionscript part = hitGameObject?.GetComponentInParent<partconditionscript>();
				sprayscript spray = hitGameObject?.GetComponentInParent<sprayscript>();

				// Return early if hit GameObject has no partconditionscript or sprayscript.
				if (part == null && spray == null)
					return;

				Color objectColor = new Color();
				if (spray != null)
				{
					objectColor = spray.color.color;
				}
				else
				{
					foreach (Renderer Renderer in part.renderers)
					{
						if (Renderer.material == null)
							continue;

						objectColor = Renderer.material.color;
					}
				}

				objectColor.a = 1;
				Colour.SetColour(objectColor);
				CacheColourTexture();
			}

			if (Input.GetKeyDown(Services.Keybinds.GetKeyByAction((int)Keybinds.Inputs.action2).AssignedKey))
			{
				GameObject hitGameObject = Raycast();
				partconditionscript part = hitGameObject?.GetComponentInParent<partconditionscript>();
				sprayscript spray = hitGameObject?.GetComponentInParent<sprayscript>();

				// Return early if hit GameObject has no partconditionscript or sprayscript.
				if (part == null && spray == null)
					return;

				if (spray != null)
					spray.color.color = Colour.GetColour();
				else
					GameUtilities.Paint(Colour.GetColour(), part);
			}
		}

		private void CacheColourTexture()
		{
			_cachedPreview = GUIExtensions.ColorTexture(1, 1, Colour.GetColour());
		}

		private void OnMenuToggle(bool show)
		{
			if (show) return;
			CacheColourTexture();
		}
	}
}
