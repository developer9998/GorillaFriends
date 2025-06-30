using HarmonyLib;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine), "UpdateLine"), HarmonyWrapSafe]
    internal class LineUpdatePatch
    {
        public static void Prefix(GorillaPlayerScoreboardLine __instance)
        {
            try
            {
                FriendButton friendButton = __instance.GetComponent<FriendButton>();
                friendButton?.InitializeWithLine();
            }
            catch
            {

            }
        }
    }
}
