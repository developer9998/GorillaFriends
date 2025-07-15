using HarmonyLib;
using UnityEngine;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine), nameof(GorillaPlayerScoreboardLine.InitializeLine)), HarmonyWrapSafe]
    internal class LineInitializePatch
    {
        public static void Prefix(GorillaPlayerScoreboardLine __instance)
        {
            bool setActive = !__instance.linePlayer.IsLocal;

            foreach (Transform child in __instance.transform)
            {
                GameObject gameObject = child.gameObject;
                if (gameObject.name == "FriendButton")
                {
                    if (gameObject.activeSelf != setActive) gameObject.SetActive(setActive);
                    break;
                }
            }
        }
    }
}
