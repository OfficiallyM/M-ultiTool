using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Extensions
{
    internal static class ColorExtensions
    {
        /// <summary>
        /// Change brightness level of a color.
        /// </summary>
        /// <param name="color">Base color</param>
        /// <param name="factor">Brightness factor between -1 and 1</param>
        /// <returns></returns>
        public static Color ChangeBrightness(this Color color, float factor)
        {
            float red = color.r * 255;
            float green = color.g * 255;
            float blue = color.b * 255;

            if (factor < 0)
            {
                factor = 1 + factor;
                red *= factor;
                green *= factor;
                blue *= factor;
            }
            else
            {
                red = (255 - red) * factor + red;
                green = (255 - green) * factor + green;
                blue = (255 - blue) * factor + blue;
            }

            return new Color(red / 255, green / 255, blue / 255);
        }
    }
}
