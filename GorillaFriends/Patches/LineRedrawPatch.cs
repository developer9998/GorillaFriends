﻿using HarmonyLib;
using UnityEngine;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard), nameof(GorillaScoreBoard.RedrawPlayerLines)), HarmonyWrapSafe, HarmonyPriority(440)]
    internal class LineRedrawPatch
    {
        private static bool Prefix(GorillaScoreBoard __instance)
        {
            if (Main.m_bScoreboardTweakerMode)
            {
                return true;
            }

            __instance.stringBuilder.Clear();
            __instance.stringBuilder.Append(__instance.GetBeginningString());
            __instance.buttonStringBuilder.Clear();
            bool nametagsEnabled = KIDManager.CheckFeatureSettingEnabled(EKIDFeatures.Custom_Nametags);
            for (int i = 0; i < __instance.lines.Count; i++)
            {
                try
                {
                    if (!__instance.lines[i].gameObject.activeInHierarchy) continue;

                    __instance.lines[i].GetComponent<RectTransform>().localPosition = new Vector3(0f, __instance.startingYValue - __instance.lineHeight * i, 0f);
                    if (__instance.lines[i].linePlayer == null || !__instance.lines[i].linePlayer.InRoom) continue;

                    __instance.stringBuilder.AppendLine().Append(" ");

                    bool isLocalPlayer = __instance.lines[i].linePlayer.IsLocal;
                    string playerId = __instance.lines[i].linePlayer.UserId;
                    string playerName = nametagsEnabled ? __instance.lines[i].playerNameVisible : __instance.lines[i].linePlayer.DefaultName;

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
                        if (__instance.lines[i].reportButton.isActiveAndEnabled)
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
