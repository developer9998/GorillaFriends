using HarmonyLib;
using UnityEngine;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.UpdateName), typeof(bool)), HarmonyWrapSafe, HarmonyPriority(440)] // 400 is "normal" priority
    public class UpdateNamePatch
    {
        public static void Postfix(VRRig __instance)
        {
            bool isLocalRig = __instance.isOfflineVRRig;
            NetPlayer creator = isLocalRig ? NetworkSystem.Instance.GetLocalPlayer() : __instance.Creator;
            string userId = creator.UserId;

            bool useBlackOutline = false;

            if (!isLocalRig && Main.IsInFriendList(userId))
                __instance.playerText1.color = Main.m_clrFriend;
            else if (Main.IsVerified(userId))
                __instance.playerText1.color = Main.m_clrVerified;
            else if (!isLocalRig && !Main.NeedToCheckRecently(userId) && Main.HasPlayedWithUsRecently(userId) == Main.eRecentlyPlayed.Before)
                __instance.playerText1.color = Main.m_clrPlayedRecently;
            else
            {
                __instance.playerText1.color = Color.white;
                useBlackOutline = true;
            }

            if (useBlackOutline) __instance.playerText2.color = Color.black;
            else
            {
                Color.RGBToHSV(__instance.playerText1.color, out float H, out float S, out float V);
                V *= 0.07f;
                __instance.playerText2.color = Color.HSVToRGB(H, S, V);
            }
        }
    }
}
