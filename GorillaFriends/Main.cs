using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaFriends
{
    [BepInPlugin(Constants.modGUID, Constants.modName, Constants.modVersion)]
    public class Main : BaseUnityPlugin
    {
        public enum eRecentlyPlayed : byte
        {
            Never = 0,
            Before = 1,
            Now = 2,
        }

        internal static Main m_hInstance = null;
        internal static bool m_bScoreboardTweakerMode = false;
        internal static HashSet<string> m_listVerifiedUserIds = [];
        internal static HashSet<string> m_listCurrentSessionFriends = [];
        internal static HashSet<string> m_listCurrentSessionRecentlyChecked = [];
        internal static Dictionary<string, eRecentlyPlayed> m_listCurrentSessionRecentCheckCache = [];
        internal static HashSet<GorillaScoreBoard> m_listScoreboards = [];
        internal static void Log(string msg) => m_hInstance.Logger.LogMessage(msg);

        // Config
        public static Color m_clrFriend { get; internal set; } = new Color(0.8f, 0.5f, 0.9f, 1.0f);
        internal static string s_clrFriend;
        public static Color m_clrVerified { get; internal set; } = new Color(0.5f, 1.0f, 0.5f, 1.0f);
        internal static string s_clrVerified;
        public static Color m_clrPlayedRecently { get; internal set; } = new Color(1.0f, 0.67f, 0.67f, 1.0f);
        internal static string s_clrPlayedRecently;

        // These are little settings for us
        internal static byte moreTimeIfWeLagging = 5; // In case our game froze for a second or more
        internal static int howMuchSecondsIsRecently = 259200; // Just a time, equal to 3 days

        public void Awake()
        {
            m_hInstance = this;
            WebVerified.LoadListOfVerified();
            Harmony.CreateAndPatchAll(typeof(Main).Assembly, Constants.modGUID);

            var cfg = new ConfigFile(Path.Combine(Paths.ConfigPath, "GorillaFriends.cfg"), true);
            moreTimeIfWeLagging = cfg.Bind("Timings", "MoreTimeOnLag", (byte)5, "This is a little settings for us in case our game froze for a second or more").Value;
            howMuchSecondsIsRecently = cfg.Bind("Timings", "RecentlySeconds", 259200, "How much is \"recently\"?").Value;
            if (howMuchSecondsIsRecently < moreTimeIfWeLagging) howMuchSecondsIsRecently = moreTimeIfWeLagging;
            m_clrPlayedRecently = cfg.Bind("Colors", "RecentlyPlayedWith", m_clrPlayedRecently, "Color of \"Recently played with ...\"").Value;
            m_clrFriend = cfg.Bind("Colors", "Friend", m_clrFriend, "Color of FRIEND!").Value;

            byte[] clrizer = { (byte)(m_clrFriend.r * 255), (byte)(m_clrFriend.g * 255), (byte)(m_clrFriend.b * 255) };
            s_clrFriend = "<color=#" + ByteArrayToHexCode(clrizer) + ">";

            clrizer[0] = (byte)(m_clrVerified.r * 255); clrizer[1] = (byte)(m_clrVerified.g * 255); clrizer[2] = (byte)(m_clrVerified.b * 255);
            s_clrVerified = "<color=#" + ByteArrayToHexCode(clrizer) + ">";

            clrizer[0] = (byte)(m_clrPlayedRecently.r * 255); clrizer[1] = (byte)(m_clrPlayedRecently.g * 255); clrizer[2] = (byte)(m_clrPlayedRecently.b * 255);
            s_clrPlayedRecently = "<color=#" + ByteArrayToHexCode(clrizer) + ">";

            RoomSystem.JoinedRoomEvent += OnJoinedRoom;
            RoomSystem.PlayerJoinedEvent += OnPlayerJoined;
            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
            RoomSystem.LeftRoomEvent += OnLeftRoom;
        }

        public void OnJoinedRoom()
        {
            Log("Joined");

            foreach(NetPlayer netPlayer in NetworkSystem.Instance.PlayerListOthers)
            {
                OnPlayerJoined(netPlayer);
            }
        }

        public void OnPlayerJoined(NetPlayer netPlayer)
        {
            Log($"+ {netPlayer}");

            string userId = netPlayer.UserId;

            bool updateName = false;

            if (NeedToCheckRecently(userId) && m_listCurrentSessionRecentlyChecked.Add(userId))
            {
                eRecentlyPlayed hasPlayedBefore = HasPlayedWithUsRecently(userId);
                m_listCurrentSessionRecentCheckCache.Add(userId, hasPlayedBefore);

                DateTime now = DateTime.Now;
                DateTimeOffset dateTimeOffset = (DateTimeOffset)now;
                long unixTimeSeconds = dateTimeOffset.ToUnixTimeSeconds();

                string key = string.Concat("pd_", userId);

                if (hasPlayedBefore == eRecentlyPlayed.Before)
                    PlayerPrefs.SetString(key, (unixTimeSeconds + moreTimeIfWeLagging).ToString());
                else
                    PlayerPrefs.SetString(key, unixTimeSeconds.ToString());

                Log($"{netPlayer.NickName}/{userId} met {hasPlayedBefore} on {now.ToShortDateString()} at {now.ToLongTimeString()}");

                updateName = true;
            }

            if (IsFriend(userId) && !IsInFriendList(userId) && m_listCurrentSessionFriends.Add(userId))
            {
                updateName = true;
            }

            if (IsVerified(userId))
            {
                updateName = true;
            }

            if (updateName && GorillaParent.instance is GorillaParent gorillaParent && gorillaParent.vrrigDict.TryGetValue(netPlayer, out VRRig playerRig))
                playerRig.UpdateName();
        }

        public void OnPlayerLeft(NetPlayer netPlayer)
        {
            Log($"- {netPlayer}");

            string userId = netPlayer.UserId;

            if (!NeedToCheckRecently(userId))
            {
                m_listCurrentSessionRecentlyChecked.Remove(userId);
                m_listCurrentSessionRecentCheckCache.Remove(userId);
            }

            if (IsInFriendList(userId))
                m_listCurrentSessionFriends.Remove(userId);
        }

        public void OnLeftRoom()
        {
            Log("Left");

            try
            {
                m_listScoreboards.Clear();
                m_listCurrentSessionFriends.Clear();

                m_listCurrentSessionRecentlyChecked.Clear();
                m_listCurrentSessionRecentCheckCache.Clear();
            }
            catch
            {
                // Who knows what's gonna happen, lol?
                // Should be safe but lets be honest -
                //   we dont wanna ruin someone's experience because of us!

                // Enid, we never really knew eachother anyway
                // Maybe we always saw right through eachother anyway
                // But, Enid, we never really knew eachother anyway, ay!
            }
        }


        void OnScoreboardTweakerStart()
        {
            m_bScoreboardTweakerMode = true;
        }

        void OnScoreboardTweakerProcessedPre(GorillaScoreBoard scoreboard)
        {
            int linesCount = scoreboard.lines.Count();
            for (int i = 0; i < linesCount; ++i)
            {
                foreach (Transform t in scoreboard.lines[i].transform)
                {
                    if (t.name == "Mute Button")
                    {
                        GameObject myFriendButton = GameObject.Instantiate(t.gameObject);
                        if (myFriendButton != null) // Who knows...
                        {
                            myFriendButton.transform.GetChild(0).localScale = new Vector3(0.032f, 0.032f, 1.0f);
                            myFriendButton.transform.GetChild(0).name = "Friend Text";
                            myFriendButton.transform.parent = scoreboard.lines[i].transform;
                            myFriendButton.transform.name = "FriendButton";
                            myFriendButton.transform.localPosition = new Vector3(18.0f, 0.0f, 0.0f);

                            var controller = myFriendButton.GetComponent<GorillaPlayerLineButton>();
                            if (controller != null)
                            {
                                FriendButton myFriendController = myFriendButton.AddComponent<FriendButton>();
                                myFriendController.parentLine = scoreboard.lines[i];
                                myFriendController.offText = "ADD\nFRIEND";
                                myFriendController.onText = "FRIEND!";
                                myFriendController.myText = controller.myText;
                                myFriendController.myText.text = myFriendController.offText;
                                myFriendController.offMaterial = controller.offMaterial;
                                myFriendController.onMaterial = new Material(controller.offMaterial);
                                myFriendController.onMaterial.color = Main.m_clrFriend;

                                GameObject.Destroy(controller);
                            }

                            myFriendButton.transform.localRotation = Quaternion.identity;
                            myFriendButton.transform.localScale = new Vector3(60.0f, t.localScale.y, 0.25f * t.localScale.z);
                            myFriendButton.transform.localPosition = new Vector3(-74.0f, 0.0f, 0.0f); // Should be -77, but i want more space between Mute and Friend button

                            myFriendButton.transform.GetChild(0).GetComponent<Text>().color = Color.clear;
                            GameObject.Destroy(myFriendButton.transform.GetComponent<MeshRenderer>());
                        }
                        break; // next line
                    }
                }
            }
        }

        private static string ByteArrayToHexCode(byte[] arr)
        {
            StringBuilder hex = new StringBuilder(arr.Length * 2);
            foreach (byte b in arr)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }

        public static bool IsVerified(string userId)
        {
            return m_listVerifiedUserIds.Contains(userId);
        }

        public static bool IsFriend(string userId)
        {
            return (PlayerPrefs.GetInt(userId + "_friend", 0) != 0);
        }

        public static bool IsInFriendList(string userId)
        {
            return m_listCurrentSessionFriends.Contains(userId);
        }

        public static bool NeedToCheckRecently(string userId)
        {
            return !m_listCurrentSessionRecentlyChecked.Contains(userId);
        }

        public static eRecentlyPlayed HasPlayedWithUsRecently(string userId)
        {
            if (m_listCurrentSessionRecentCheckCache.TryGetValue(userId, out eRecentlyPlayed cache))
                return cache;

            string key = string.Concat("pd_", userId);
            long lastPlayedTime = long.Parse(PlayerPrefs.GetString(key, "0"), CultureInfo.InvariantCulture);
            if (lastPlayedTime == 0) return eRecentlyPlayed.Never;

            long currentTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();

            if (NetworkSystem.Instance.netPlayerCache.Find(player => player.UserId == userId) is NetPlayer netPlayer)
            {
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(lastPlayedTime).DateTime.ToLocalTime();
                Log($"{netPlayer.NickName} last met on {dateTime.ToShortDateString()} at {dateTime.ToLongTimeString()}");
            }
            
            if (lastPlayedTime > currentTime - moreTimeIfWeLagging && lastPlayedTime <= currentTime) return eRecentlyPlayed.Now;
            return ((lastPlayedTime + howMuchSecondsIsRecently) > currentTime) ? eRecentlyPlayed.Before : eRecentlyPlayed.Never;
        }
    }
}