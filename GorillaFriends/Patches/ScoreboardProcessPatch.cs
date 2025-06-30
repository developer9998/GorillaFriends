using GorillaExtensions;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard), "Start"), HarmonyWrapSafe]
    internal class ScoreboardProcessPatch
    {
        public static void Prefix(GorillaScoreBoard __instance)
        {
            try
            {
                Main.Log($"Processing a scoreboard: {__instance.transform.GetPath().TrimStart('/')}");

                if (Main.m_listScoreboards.Add(__instance))
                {
                    __instance.boardText.richText = true;

                    var ppTmp = __instance.buttonText.transform.localPosition;
                    var sd = __instance.buttonText.rectTransform.sizeDelta;
                    __instance.buttonText.transform.localPosition = new Vector3(
                        ppTmp.x - 3.0f,
                        ppTmp.y,
                        ppTmp.z
                    );
                    __instance.buttonText.rectTransform.sizeDelta = new Vector2(sd.x + 4.0f, sd.y);
                    __instance.buttonText.overflowMode = TMPro.TextOverflowModes.Overflow;
                    __instance.buttonText.enableWordWrapping = false;

                    // For the new Gorilla Tag versions
                    if (Main.m_bScoreboardTweakerMode) return;

                    int linesCount = __instance.lines.Count();
                    Main.Log("Got a scoreboard with " + linesCount + " lines!");
                    for (int i = 0; i < linesCount; ++i)
                    {
                        Transform muteButton = __instance.lines[i]?.muteButton?.transform;

                        if (muteButton == null || !muteButton)
                        {
                            foreach (Transform child in __instance.lines[i].transform)
                            {
                                if (child.name == "Mute Button" || (child.TryGetComponent(out GorillaPlayerLineButton button) && button.buttonType == GorillaPlayerLineButton.ButtonType.Mute))
                                {
                                    muteButton = child;
                                    break;
                                }
                            }
                        }

                        if (muteButton != null && muteButton)
                        {
                            GameObject myFriendButton = Object.Instantiate(muteButton.gameObject);
                            if (myFriendButton != null) // Who knows...
                            {
                                muteButton.localPosition = new Vector3(17.5f, 0.0f, 0.0f); // Move MuteButton a bit to the right
                                myFriendButton.transform.parent = __instance.lines[i].transform;
                                myFriendButton.transform.name = "FriendButton";
                                myFriendButton.transform.localPosition = new Vector3(3.8f, 0.0f, 0.0f);
                                myFriendButton.transform.localScale = muteButton.localScale;
                                myFriendButton.transform.rotation = muteButton.rotation;
                                if (myFriendButton.TryGetComponent(out GorillaPlayerLineButton controller)) // magic
                                {
                                    FriendButton myFriendController = myFriendButton.AddComponent<FriendButton>();
                                    myFriendController.parentLine = controller.parentLine;
                                    myFriendController.offText = "ADD\nFRIEND";
                                    myFriendController.onText = "FRIEND!";
                                    myFriendController.myText = controller.myText;
                                    myFriendController.myText.text = myFriendController.offText;
                                    myFriendController.offMaterial = controller.offMaterial;
                                    myFriendController.onMaterial = new Material(controller.offMaterial)
                                    {
                                        color = Main.m_clrFriend
                                    };
                                    Object.Destroy(controller); // We are not muting friends!!!
                                }
                            }
                        }
                    }

                    __instance.RedrawPlayerLines(); // Check
                    return;
                }

                Main.Log("Already processed. Skipping.");
            }
            catch
            {

            }
        }
    }
}
