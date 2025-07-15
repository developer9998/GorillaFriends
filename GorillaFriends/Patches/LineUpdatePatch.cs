using HarmonyLib;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine), nameof(GorillaPlayerScoreboardLine.UpdateLine)), HarmonyWrapSafe]
    internal class LineUpdatePatch
    {
        public static void Prefix(GorillaPlayerScoreboardLine __instance)
        {
            FriendButton friendButton = __instance.GetComponent<FriendButton>();
            friendButton?.InitializeWithLine();
        }
    }
}
