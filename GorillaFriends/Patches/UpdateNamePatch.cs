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

            if (!isLocalRig && Main.IsInFriendList(userId))
            {
                mainColour = Main.m_clrFriend;
            }
            else if (Main.IsVerified(userId))
            {
                mainColour = Main.m_clrVerified;
            }
            else if (!isLocalRig && !Main.NeedToCheckRecently(userId) && Main.HasPlayedWithUsRecently(userId) is var hasPlayedBefore && hasPlayedBefore.recentlyPlayed == Main.eRecentlyPlayed.Before)
            {
                mainColour = Color.Lerp(Color.white, Main.m_clrPlayedRecently, hasPlayedBefore.value);
            }

            __instance.playerText1.color = mainColour;
        }
    }
}
