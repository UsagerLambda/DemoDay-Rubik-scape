using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RubiksCubeInteraction : MonoBehaviour
{
    [Header("Références")]
    public RubikGen rubikGen;
    public XRDirectInteractor leftHand;
    public XRDirectInteractor rightHand;
    
    [Header("Paramètres de rotation")]
    public float rotationSpeed = 300f;
    public float snapAngleThreshold = 45f;
    
    private GameObject selectedCube;
    private GameObject[] selectedLayer;
    private Vector3 rotationAxis;
    private int currentLayer;
    private bool isRotating;
    private Vector3 previousHandPosition;
    private Quaternion previousHandRotation;
    private InteractionMode currentMode;
    
    private enum InteractionMode
    {
        None,
        RightHandClassic,
        LeftHandAxisBased
    }

    private void Start()
    {
        // Configuration des événements d'interaction
        SetupInteractionEvents();
    }

    private void SetupInteractionEvents()
    {
        rightHand.selectEntered.AddListener(OnRightHandSelectEntered);
        rightHand.selectExited.AddListener(OnRightHandSelectExited);
        leftHand.selectEntered.AddListener(OnLeftHandSelectEntered);
        leftHand.selectExited.AddListener(OnLeftHandSelectExited);
    }

    private void OnRightHandSelectEntered(SelectEnterEventArgs args)
    {
        if (currentMode != InteractionMode.None) return;
        
        GameObject grabbed = args.interactableObject.transform.gameObject;
        if (grabbed.CompareTag("CubePiece")) // Vérifie si c'est un morceau du cube
        {
            currentMode = InteractionMode.RightHandClassic;
            selectedCube = grabbed;
            previousHandRotation = rightHand.transform.rotation;
        }
    }

    private void OnRightHandSelectExited(SelectExitEventArgs args)
    {
        if (currentMode == InteractionMode.RightHandClassic)
        {
            currentMode = InteractionMode.None;
            selectedCube = null;
            SnapToNearestRotation();
        }
    }

    private void OnLeftHandSelectEntered(SelectEnterEventArgs args)
    {
        if (currentMode != InteractionMode.None) return;
        
        GameObject grabbed = args.interactableObject.transform.gameObject;
        if (grabbed.CompareTag("CubePiece"))
        {
            currentMode = InteractionMode.LeftHandAxisBased;
            selectedCube = grabbed;
            
            (rotationAxis, currentLayer) = GetCubeAxisAndLayer(grabbed);
            selectedLayer = GetLayerCubes(rotationAxis, currentLayer);
            
            previousHandPosition = leftHand.transform.position;
        }
    }

    private void OnLeftHandSelectExited(SelectExitEventArgs args)
    {
        if (currentMode == InteractionMode.LeftHandAxisBased)
        {
            currentMode = InteractionMode.None;
            selectedCube = null;
            selectedLayer = null;
            SnapToNearestRotation();
        }
    }

    private void Update()
    {
        if (currentMode == InteractionMode.RightHandClassic)
        {
            HandleClassicRotation();
        }
        else if (currentMode == InteractionMode.LeftHandAxisBased)
        {
            HandleAxisBasedRotation();
        }
    }

    private void HandleClassicRotation()
    {
        if (selectedCube == null) return;

        Quaternion deltaRotation = Quaternion.Inverse(previousHandRotation) * 
                                 rightHand.transform.rotation;
        
        selectedCube.transform.rotation *= deltaRotation;
        previousHandRotation = rightHand.transform.rotation;
    }

    private void HandleAxisBasedRotation()
    {
        if (selectedLayer == null || selectedLayer.Length == 0) return;

        Vector3 currentHandPosition = leftHand.transform.position;
        Vector3 movement = currentHandPosition - previousHandPosition;
        
        Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, rotationAxis);
        
        float rotationAmount = projectedMovement.magnitude * rotationSpeed * 
                             Mathf.Sign(Vector3.Dot(projectedMovement, 
                             Vector3.Cross(rotationAxis, Vector3.up)));

        foreach (GameObject cube in selectedLayer)
        {
            if (cube != null)
            {
                cube.transform.RotateAround(
                    rubikGen.transform.position,
                    rotationAxis,
                    rotationAmount
                );
            }
        }

        previousHandPosition = currentHandPosition;
    }

    private void SnapToNearestRotation()
    {
        if (selectedLayer == null) return;

        foreach (GameObject cube in selectedLayer)
        {
            if (cube != null)
            {
                Vector3 currentRotation = cube.transform.rotation.eulerAngles;
                Vector3 snappedRotation = new Vector3(
                    Mathf.Round(currentRotation.x / 90f) * 90f,
                    Mathf.Round(currentRotation.y / 90f) * 90f,
                    Mathf.Round(currentRotation.z / 90f) * 90f
                );
                cube.transform.rotation = Quaternion.Euler(snappedRotation);
            }
        }
    }

    private GameObject[] GetLayerCubes(Vector3 axis, int layerIndex)
    {
        GameObject[] layerCubes = new GameObject[rubikGen.cubeSize * rubikGen.cubeSize];
        int index = 0;

        if (axis == Vector3.right)
        {
            for (int y = 0; y < rubikGen.cubeSize; y++)
                for (int z = 0; z < rubikGen.cubeSize; z++)
                    layerCubes[index++] = rubikGen.cubeArray[layerIndex, y, z];
        }
        else if (axis == Vector3.up)
        {
            for (int x = 0; x < rubikGen.cubeSize; x++)
                for (int z = 0; z < rubikGen.cubeSize; z++)
                    layerCubes[index++] = rubikGen.cubeArray[x, layerIndex, z];
        }
        else if (axis == Vector3.forward)
        {
            for (int x = 0; x < rubikGen.cubeSize; x++)
                for (int y = 0; y < rubikGen.cubeSize; y++)
                    layerCubes[index++] = rubikGen.cubeArray[x, y, layerIndex];
        }

        return layerCubes;
    }

    private (Vector3 axis, int layer) GetCubeAxisAndLayer(GameObject cube)
    {
        // On récupère la position du cube dans la grille en parcourant le cubeArray
        for (int x = 0; x < rubikGen.cubeSize; x++)
        {
            for (int y = 0; y < rubikGen.cubeSize; y++)
            {
                for (int z = 0; z < rubikGen.cubeSize; z++)
                {
                    if (rubikGen.cubeArray[x, y, z] == cube)
                    {
                        // Détermine l'axe basé sur la position de la main par rapport au centre
                        Vector3 relativePos = cube.transform.position - rubikGen.transform.position;
                        Vector3 absPos = new Vector3(
                            Mathf.Abs(relativePos.x),
                            Mathf.Abs(relativePos.y),
                            Mathf.Abs(relativePos.z)
                        );

                        // Trouve l'axe le plus éloigné du centre
                        if (absPos.x > absPos.y && absPos.x > absPos.z)
                            return (Vector3.right, x);
                        else if (absPos.y > absPos.z)
                            return (Vector3.up, y);
                        else
                            return (Vector3.forward, z);
                    }
                }
            }
        }

        return (Vector3.zero, -1);
    }

    private void OnDestroy()
    {
        if (rightHand != null)
        {
            rightHand.selectEntered.RemoveListener(OnRightHandSelectEntered);
            rightHand.selectExited.RemoveListener(OnRightHandSelectExited);
        }
        
        if (leftHand != null)
        {
            leftHand.selectEntered.RemoveListener(OnLeftHandSelectEntered);
            leftHand.selectExited.RemoveListener(OnLeftHandSelectExited);
        }
    }
}