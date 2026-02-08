using GorillaNetworking;
using GorillaTagScripts;
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
            NetPlayer player = __instance.Creator ?? NetworkSystem.Instance.GetLocalPlayer();
            string userId = player.UserId;

            Color mainColour = Color.white;

            if (!isLocalRig && Main.IsInFriendList(userId))
            {
                mainColour = Main.m_clrFriend;
                goto SetTagColour;
            }

            if (Main.IsVerified(userId))
            {
                mainColour = Main.m_clrVerified;
                goto SetTagColour;
            }

            SubscriptionManager.SubscriptionDetails subscriptionDetails = SubscriptionManager.GetSubscriptionDetails(player);
            mainColour = (__instance.ShowGoldNameTag || (subscriptionDetails.active && subscriptionDetails.tier > 0)) ? SubscriptionManager.SUBSCRIBER_NAME_COLOR : Color.white;

            if (!isLocalRig && !Main.NeedToCheckRecently(userId) && Main.HasPlayedWithUsRecently(userId) is var hasPlayedBefore && hasPlayedBefore.recentlyPlayed == Main.eRecentlyPlayed.Before)
            {
                mainColour = Color.Lerp(Color.white, Main.m_clrPlayedRecently, hasPlayedBefore.value);
                goto SetTagColour;
            }

        SetTagColour:
            __instance.playerText1.color = mainColour;
        }
    }
}
