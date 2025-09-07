using GorillaNetworking;
using HarmonyLib;
using UnityEngine;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.UpdateName), typeof(bool)), HarmonyWrapSafe, HarmonyPriority(440)] // 400 is "normal" priority
    public class UpdateNamePatch
    {
        public static void Postfix(VRRig __instance)
        {
            bool isLocalRig = __instance.isOfflineVRRig || __instance.isLocal;
            string userId = isLocalRig ? PlayFabAuthenticator.instance.GetPlayFabPlayerId() : __instance.Creator.UserId;

            Color mainColour = Color.white;
            Color outlineColour = Color.black;

            if (!isLocalRig && Main.IsInFriendList(userId))
            {
                mainColour = Main.m_clrFriend;
                outlineColour = WithValueMultiplier(Main.m_clrFriend);
            }
            else if (Main.IsVerified(userId))
            {
                mainColour = Main.m_clrVerified;
                outlineColour = WithValueMultiplier(Main.m_clrVerified);
            }
            else if (!isLocalRig && !Main.NeedToCheckRecently(userId) && Main.HasPlayedWithUsRecently(userId) == Main.eRecentlyPlayed.Before)
            {
                mainColour = Main.m_clrPlayedRecently;
                outlineColour = WithValueMultiplier(Main.m_clrPlayedRecently);
            }

            __instance.playerText1.color = mainColour;
            //__instance.playerText2.color = outlineColour;
        }

        private static Color WithValueMultiplier(Color original, float valueMultiplier = 0.06f)
        {
            Color.RGBToHSV(original, out float H, out float S, out float V);
            V *= valueMultiplier;
            return Color.HSVToRGB(H, S, V);
        }
    }
}
