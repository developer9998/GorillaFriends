using GorillaTagScripts;
using HarmonyLib;

namespace GorillaFriends.Patches;

[HarmonyPatch(typeof(VRRig))]
internal class StupidSubscriptionPatches
{
    [HarmonyPatch(nameof(VRRig.SerializeReadShared)), HarmonyPostfix]
    public static void DataReadPatch(VRRig __instance)
    {
        __instance.ShowGoldNameTag = __instance.playerText1.color == SubscriptionManager.SUBSCRIBER_NAME_COLOR;
        __instance.UpdateName();
    }

    [HarmonyPatch(nameof(VRRig.OnSubscriptionData)), HarmonyPostfix]
    public static void DataRecievedPatch(VRRig __instance)
    {
        __instance.UpdateName();
    }
}
