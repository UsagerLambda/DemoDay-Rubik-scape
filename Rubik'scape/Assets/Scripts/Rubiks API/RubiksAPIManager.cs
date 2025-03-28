using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Accepter tous les certificats
        return true;
    }
}

public class RubiksAPIManager : MonoBehaviour
{
    public Transform ContentContainer;
    public FetchCanvas dataReceiver { get; set; }
    private const string BaseUrl = "https://rubiks-server-43tw.onrender.com";

    public IEnumerator GetAllLevels()
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/get_all_levels"))
        {
            request.certificateHandler = new AcceptAllCertificates();
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
            request.certificateHandler = new AcceptAllCertificates();
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
