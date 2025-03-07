using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiTool.Modules;
using MultiTool.Utilities.UI;
using ScottPlot;
using static snTempPlayerSync;

namespace MultiTool.Tabs
{
	internal class ShapesTab: Tab
	{
		public override string Name => "Shapes";
		public override bool HasConfigPane => true;

        // Scroll vectors.
        private Vector2 _shapesScrollPosition;
        private Vector2 _configScrollPosition;

        // Main tab variables.
        private int _maxPrimitives = 0;

        // Config variables.
        private bool _linkScale;
        private Vector3 _scale = Vector3.one;

        public override void OnRegister()
        {
            _maxPrimitives = (int)Enum.GetValues(typeof(PrimitiveType)).Cast<PrimitiveType>().Max();
        }

        public override void RenderTab(Rect dimensions)
		{
            GUILayout.BeginArea(dimensions);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            _shapesScrollPosition = GUILayout.BeginScrollView(_shapesScrollPosition);
            for (int i = 0; i < _maxPrimitives; i++)
            {
                // Skip plane, it just falls through the floor.
                if ((PrimitiveType)i == PrimitiveType.Plane) continue;

                if (GUILayout.Button(((PrimitiveType)i).ToString(), GUILayout.MaxWidth(200), GUILayout.MaxHeight(30)))
                {
                    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType)i);

					primitive = UnityEngine.Object.Instantiate(primitive, mainscript.M.player.lookPoint + Vector3.up * 0.75f, Quaternion.FromToRotation(Vector3.forward, -mainscript.M.player.transform.right));
					mainscript.M.PostSpawn(primitive);

					pickupable pickupable = primitive.AddComponent<pickupable>();
					massScript mass = primitive.AddComponent<massScript>();
					mass.SetMass(20f);
					mass.P = pickupable;
					mass.AddRB();
					primitive.GetComponent<Renderer>().material.color = Colour.GetColour();
                    primitive.transform.localScale = _scale;
                }
                GUILayout.Space(10);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
		}

        public override void RenderConfigPane(Rect dimensions)
        {
            GUILayout.BeginArea(dimensions);
            GUILayout.BeginVertical();
            _configScrollPosition = GUILayout.BeginScrollView(_configScrollPosition);

            if (GUILayout.Button(Accessibility.GetAccessibleString("Link scale axis", _linkScale)))
                _linkScale = !_linkScale;
            GUILayout.Space(10);

            if (_linkScale)
            {
                // Scale.
                GUILayout.Label("Scale:");
                float allScale = GUILayout.HorizontalSlider(_scale.x, 0.1f, 10f);
                allScale = (float)Math.Round(allScale, 2);
                allScale = (float)Math.Round(double.Parse(GUILayout.TextField(allScale.ToString())), 2);
                allScale = Mathf.Clamp(allScale, 0.1f, 100f);
                _scale = new Vector3(allScale, allScale, allScale);
            }
            else
            {
                // X.
                GUILayout.Label("Scale X:");
                float x = GUILayout.HorizontalSlider(_scale.x, 0.1f, 10f);
                x = (float)Math.Round(x, 2);
                x = (float)Math.Round(double.Parse(GUILayout.TextField(x.ToString())), 2);
                x = Mathf.Clamp(x, 0.1f, 100f);
                _scale.x = x;
                GUILayout.Space(10);

                // Y.
                GUILayout.Label("Scale Y:");
                float y = GUILayout.HorizontalSlider(_scale.y, 0.1f, 10f);
                y = (float)Math.Round(y, 2);
                y = (float)Math.Round(double.Parse(GUILayout.TextField(y.ToString())), 2);
                y = Mathf.Clamp(y, 0.1f, 100f);
                _scale.y = y;
                GUILayout.Space(10);

                // Z.
                GUILayout.Label("Scale Z:");
                float z = GUILayout.HorizontalSlider(_scale.z, 0.1f, 10f);
                z = (float)Math.Round(z, 2);
                z = (float)Math.Round(double.Parse(GUILayout.TextField(z.ToString())), 2);
                z = Mathf.Clamp(z, 0.1f, 100f);
                _scale.z = z;
            }
            GUILayout.Space(10);

            Colour.RenderColourSliders(dimensions.width, hasAlpha: true);
            GUILayout.Space(10);

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
