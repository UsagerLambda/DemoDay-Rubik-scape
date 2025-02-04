using UnityEngine;
using System.Collections;

public class Run : MonoBehaviour
{
    [SerializeField] private FetchCanvas fetchCanvas;
    [SerializeField] private GameObject curvedUnityCanvas;
    [SerializeField] private RubikGen rubikGen;
    private RubiksAPIManager apiManager;

    void Start()
    {
        apiManager = gameObject.AddComponent<RubiksAPIManager>();
        apiManager.dataReceiver = fetchCanvas;
        fetchCanvas.apiManager = apiManager;
        fetchCanvas.Initialize(curvedUnityCanvas, rubikGen);
        StartCoroutine(apiManager.GetAllLevels());
    }

    // Méthode pour appeler GetAllLevels depuis un bouton
    public void FetchLevels()
    {
        StartCoroutine(apiManager.GetAllLevels());
    }
}
