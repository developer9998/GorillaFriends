using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GorillaFriends.Extensions
{
    internal static class PlayerRigEx
    {
        public static void SetTagColour(this VRRig playerRig, Color colour, bool modifyHSV = true)
        {
            playerRig.playerText1.color = colour;
            playerRig.playerText2.color = modifyHSV ? colour.ModifyHSV(vMulti: 0.07f) : Color.black;
        }
    }
}
