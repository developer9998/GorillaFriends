using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaFriends
{
    /* Friend Button's Script */
    public class FriendButton : MonoBehaviour
    {
        public GorillaPlayerScoreboardLine parentLine = null;
        public bool initialized = false;
        public bool isOn = false;
        public string offText = "";
        public string onText = "";
        public Text myText = null;
        public Material offMaterial;
        public Material onMaterial;
        private MeshRenderer meshRenderer = null;
        private float nextUpdate = 0.0f;
        private static float nextTouch = 0.0f;

        private void Start()
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
        }

        public void Update()
        {
            NetPlayer playa = parentLine.linePlayer;
            if (nextUpdate > Time.time || parentLine.playerVRRig == null || playa == null) return;
            nextUpdate = Time.time + 0.5f;

            InitializeWithLine();

            if (playa != null && !playa.IsLocal && isOn != Main.IsInFriendList(playa.UserId))
            {
                isOn = !isOn;
                UpdateColor();

                if (!isOn)
                {
                    if (Main.IsVerified(playa.UserId))
                    {
                        parentLine.playerName.color = Main.m_clrVerified;
                        parentLine.playerVRRig.playerText1.color = Main.m_clrVerified;
                    }
                    else
                    {
                        parentLine.playerName.color = Color.white;
                        parentLine.playerVRRig.playerText1.color = Color.white;
                    }
                }
                else
                {
                    parentLine.playerName.color = Main.m_clrFriend;
                    parentLine.playerVRRig.playerText1.color = Main.m_clrFriend;
                }
            }
        }

        public void InitializeWithLine()
        {
            /* First Initialization? */
            if (initialized) return;
            initialized = true;

            string userId = parentLine.linePlayer.UserId;
            bool isLocalPlaya = parentLine.linePlayer.IsLocal;

            gameObject.SetActive(!isLocalPlaya); // Turn-off a button that was made for us

            if (!isLocalPlaya && Main.IsInFriendList(userId))
            {
                isOn = true;
                UpdateColor();
            }

            if (parentLine.playerVRRig is VRRig playerRig)
                parentLine.playerName.color = playerRig.playerText1.color;
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (nextTouch > Time.time) return;
            GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            if (component == null) return;

            nextTouch = Time.time + 0.125f;
            isOn = !isOn;
            UpdateColor();

            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, component.isLeftHand, 0.05f);
            GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
            if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, 67, component.isLeftHand, 0.05f);
            }

            if (isOn) Main.AddFriend(parentLine.linePlayer.UserId);
            else Main.RemoveFriend(parentLine.linePlayer.UserId);

            /* GT 1.1.0 */
            if (!Main.m_bScoreboardTweakerMode)
            {
                Main.Log("Initiating Scoreboard Redraw...");
                foreach (var sb in Main.m_listScoreboards)
                {
                    Main.Log("Redrawing...");
                    sb.RedrawPlayerLines();
                }
            }
            /* GT 1.1.0 */
        }
        public void UpdateColor()
        {
            if (isOn)
            {
                if (meshRenderer != null) meshRenderer.material = onMaterial;
                myText.text = onText;
            }
            else
            {
                if (meshRenderer != null) meshRenderer.material = offMaterial;
                myText.text = offText;
            }
        }
    }
}