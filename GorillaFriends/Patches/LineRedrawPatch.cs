using HarmonyLib;
using UnityEngine;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard), nameof(GorillaScoreBoard.RedrawPlayerLines)), HarmonyWrapSafe, HarmonyPriority(440)] // "440" is a little above average, but not enough to be considered higher than usual
    internal class LineRedrawPatch
    {
        private static bool Prefix(GorillaScoreBoard __instance)
        {
            if (Main.m_bScoreboardTweakerMode) return true;

            bool nametagsEnabled = KIDManager.CheckFeatureSettingEnabled(EKIDFeatures.Custom_Nametags);

            __instance.stringBuilder.Clear();
            __instance.stringBuilder.Append(__instance.GetBeginningString());
            __instance.buttonStringBuilder.Clear();
            
            for (int i = 0; i < __instance.lines.Count; i++)
            {
                GorillaPlayerScoreboardLine line = __instance.lines[i];
                if (line == null || !line || !line.gameObject.activeInHierarchy) continue;

                try
                {
                    line.GetComponent<RectTransform>().localPosition = new Vector3(0f, __instance.startingYValue - __instance.lineHeight * i, 0f);
                    if (line.linePlayer == null || !line.linePlayer.InRoom) continue;

                    __instance.stringBuilder.AppendLine().Append(" ");

                    bool isLocalPlayer = line.linePlayer.IsLocal;
                    string playerId = line.linePlayer.UserId;
                    string playerName = nametagsEnabled ? line.playerNameVisible : line.linePlayer.DefaultName;

                    if (!isLocalPlayer && Main.IsInFriendList(playerId))
                        __instance.stringBuilder.Append(Main.s_clrFriend).Append(playerName).Append("</color>");
                    else if (Main.IsVerified(playerId))
                        __instance.stringBuilder.Append(Main.s_clrVerified).Append(playerName).Append("</color>");
                    else if (!isLocalPlayer && !Main.NeedToCheckRecently(playerId) && Main.HasPlayedWithUsRecently(playerId) == Main.eRecentlyPlayed.Before)
                        __instance.stringBuilder.Append(Main.s_clrPlayedRecently).Append(playerName).Append("</color>");
                    else
                        __instance.stringBuilder.Append(playerName);

                    if (!isLocalPlayer)
                    {
                        if (line.reportButton.isActiveAndEnabled)
                        {
                            __instance.buttonStringBuilder.AppendLine("FRIEND       MUTE                      REPORT");
                        }
                        else
                        {
                            __instance.buttonStringBuilder.AppendLine("FRIEND       MUTE      HATE SPEECH    TOXICITY      CHEATING      CANCEL");
                        }
                    }
                    else
                    {
                        __instance.buttonStringBuilder.AppendLine();
                    }
                }
                catch
                {

                }
            }

            __instance.boardText.text = __instance.stringBuilder.ToString();
            __instance.buttonText.text = __instance.buttonStringBuilder.ToString();
            __instance._isDirty = false;

            return false;
        }
    }
}
