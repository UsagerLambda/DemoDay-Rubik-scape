using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.UI;

public class RubiksAPIManager : MonoBehaviour
{
    public Transform ContentContainer;
    public FetchCanvas dataReceiver { get; set; }
    private const string BaseUrl = "https://game.rubikscape.online";

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
                EnableAllLevelButtons();
                yield break;
            }

            APIResponse response = JsonConvert.DeserializeObject<APIResponse>(request.downloadHandler.text);
            if (response?.level == null)
            {
                Debug.LogError("Received null level from API");
                EnableAllLevelButtons();
                yield break;
            }

            dataReceiver?.HandleSingleLevel(response.level);
        }
    }

    private void EnableAllLevelButtons() {
        foreach (Transform child in ContentContainer) {
            Button button = child.transform
                .Find("SelectButton/LevelSelectorButton")
                .GetComponent<Button>();

            button.interactable = true;
        }
    }
}
