using HarmonyLib;
using UnityEngine;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine), "InitializeLine"), HarmonyWrapSafe]
    internal class LineInitializePatch
    {
        public static void Prefix(GorillaPlayerScoreboardLine __instance)
        {
            try
            {
                //FriendButton friendButton = __instance.GetComponent<FriendButton>();
                //friendButton?.InitializeWithLine();
                foreach (Component component in __instance.GetComponentsInChildren<FriendButton>(true))
                {
                    component.gameObject.SetActive(!__instance.linePlayer.IsLocal);
                    break;
                }
            }
            catch
            {

            }
        }
    }
}
