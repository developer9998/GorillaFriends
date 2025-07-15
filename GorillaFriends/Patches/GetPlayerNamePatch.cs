using HarmonyLib;
using GorillaNetworking;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(PlayFabAuthenticator), nameof(PlayFabAuthenticator.GetPlayerDisplayName)), HarmonyWrapSafe, HarmonyPriority(Priority.Low)]
    internal class GetPlayerNamePatch
    {
        public static void Prefix()
        {
            VRRig localRig = VRRig.LocalRig ?? GorillaTagger.Instance.offlineVRRig;
            if (localRig is not null) localRig.UpdateName();
        }
    }
}
