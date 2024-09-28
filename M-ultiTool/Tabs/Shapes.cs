using MultiTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static settingsscript;
using UnityEngine;
using MultiTool.Modules;

namespace MultiTool.Tabs
{
	internal class ShapesTab: Tab
	{
		public override string Name => "Shapes";
		public override bool HasConfigPane => true;
		public override void RenderTab(Dimensions dimensions)
		{
			GUI.skin.button.wordWrap = true;

			float itemWidth = 140f;
			float itemHeight = 40f;
			float initialRowX = dimensions.x + 10f;
			float itemX = initialRowX;
			float itemY = dimensions.y - 40f;

			int maxRowItems = Mathf.FloorToInt(dimensions.width / (itemWidth + 10f));

			Dictionary<string, string> shapes = new Dictionary<string, string>()
					{
						{ "cube", "Cube" },
						{ "sphere", "Sphere" },
						{ "cylinder", "Cylinder" }
					};

			for (int i = 0; i < shapes.Count(); i++)
			{
				KeyValuePair<string, string> shape = shapes.ElementAt(i);

				itemX += itemWidth + 10f;

				if (i % maxRowItems == 0)
				{
					itemX = initialRowX;
					itemY += itemHeight + 10f;
				}

				if (GUI.Button(new Rect(itemX, itemY, itemWidth, itemHeight), shape.Value))
				{
					GameObject primitive = null;

					switch (shape.Key)
					{
						case "cube":
							primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
							break;
						case "sphere":
							primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
							break;
						case "cylinder":
							primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
							break;
					}


					if (primitive != null)
					{
						pickupable pickupable = primitive.AddComponent<pickupable>();
						massScript mass = primitive.AddComponent<massScript>();
						mass.SetMass(20f);
						mass.P = pickupable;
						mass.AddRB();

						primitive.GetComponent<Renderer>().material.color = GUIRenderer.color;
						primitive.transform.localScale = GUIRenderer.scale;

						mainscript.s.Spawn(primitive, -1);
					}
				}
			}
		}
	}
}
