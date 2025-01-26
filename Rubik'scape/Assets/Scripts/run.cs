using UnityEngine;
using System.Collections;

public class run : MonoBehaviour
{
    void Start()
    {
        RubiksAPIManager apiManager = gameObject.AddComponent<RubiksAPIManager>();
        StartCoroutine(apiManager.GetAllLevels());
    }
}