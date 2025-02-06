using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class RubiksAPIManager : MonoBehaviour
{
    public FetchCanvas dataReceiver { get; set; }
    private const string BaseUrl = "https://rubiks-server.onrender.com";

    public IEnumerator GetAllLevels()
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/get_all_levels"))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Request failed: {request.error}");
                yield break;
            }

            APIResponse response = JsonConvert.DeserializeObject<APIResponse>(request.downloadHandler.text);
            if (dataReceiver != null)
            {
                dataReceiver.HandleAllLevels(response.levels);
            }
            else
            {
                Debug.LogError("DataReceiver is not set in RubiksAPIManager");
            }
        }
    }

    public IEnumerator GetLevel(string levelId)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/get_level/{levelId}"))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Request failed: {request.error}");
                yield break;
            }

            APIResponse response = JsonConvert.DeserializeObject<APIResponse>(request.downloadHandler.text);
            if (response?.level == null)
            {
                Debug.LogError("Received null level from API");
                yield break;
            }

            dataReceiver?.HandleSingleLevel(response.level);
        }
    }
}
