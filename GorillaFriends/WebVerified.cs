using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace GorillaFriends
{
    class WebVerified
    {
        public const string m_szURL = "https://raw.githubusercontent.com/developer9998/GorillaFriends/main/gorillas.verified";

        public static async void LoadListOfVerified()
        {
            using UnityWebRequest request = UnityWebRequest.Get(m_szURL);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await operation;

            string result = request.downloadHandler.text;
            using StringReader reader = new(result);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Main.m_listVerifiedUserIds.Add(line);
            }

            try
            {
                if (GorillaTagger.hasInstance && GorillaTagger.Instance.offlineVRRig is VRRig localRig && localRig) localRig.UpdateName();
            }
            catch
            {

            }
        }
    }
}
