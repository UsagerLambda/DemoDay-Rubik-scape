using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RubiksAPIManager : MonoBehaviour
{
    private string baseUrl = "https://rubiks-server.onrender.com";

    public IEnumerator GetAllLevels()
    {
        string url = $"{baseUrl}/get_all_levels";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                APIResponse response = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
                foreach (Level level in response.levels)
                    Debug.Log($"Level: {level.name} (ID: {level.id})");
            }
            else
                Debug.LogError($"Request failed: {request.error}");
        }
    }

    public IEnumerator GetLevel(string levelId)
    {
        string url = $"{baseUrl}/get_level/{levelId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                APIResponse response = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
                Level level = response.level;
                Debug.Log($"Loaded Level: {level.name}");
                Debug.Log($"Cube size: {level.cube_size}");
                Debug.Log($"Faces data: {level.faces_data}");
            }
            else
                Debug.LogError($"Request failed: {request.error}");
        }
    }
}