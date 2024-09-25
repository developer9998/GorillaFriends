using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GorillaFriends.Extensions
{
    internal static class ColourEx
    {
        public static Color ModifyHSV(this Color colour, float hMulti = 1, float sMulti = 1, float vMulti = 1)
        {
            Color.RGBToHSV(colour, out float H, out float S, out float V);
            H *= hMulti;
            S *= sMulti;
            V *= vMulti;
            return Color.HSVToRGB(H, S, V);
        }
    }
}
