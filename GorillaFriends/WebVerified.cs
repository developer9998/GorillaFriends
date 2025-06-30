using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace GorillaFriends
{
    class WebVerified
    {
        public const string m_szURL = "https://raw.githubusercontent.com/Not-A-Bird-07/GorillaFriends/refs/heads/main/gorillas.verified";

        public static async void LoadListOfVerified()
        {
            using UnityWebRequest request = UnityWebRequest.Get(m_szURL);

            TaskCompletionSource<UnityWebRequest> completionSource = new TaskCompletionSource<UnityWebRequest>();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ => completionSource.SetResult(request);

            await completionSource.Task;

            string result = request.downloadHandler.text;
            using StringReader reader = new StringReader(result);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Main.m_listVerifiedUserIds.Add(line);
            }
        }
    }
}
