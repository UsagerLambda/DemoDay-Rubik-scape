using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Meta.XR;

public class RubikRotation : MonoBehaviour
{
    private RubikGen rubikGen;
    private bool isRotating = false;
    private float rotationSpeed = 90f;
    private float pinchThreshold = 0.8f;

    public OVRHand rightHand;
    public OVRHand leftHand;
    public OVRSkeleton rightHandSkeleton;
    public OVRSkeleton leftHandSkeleton;

    private bool wasRightPinching = false;
    private bool wasLeftPinching = false;
    private Vector3 pinchStartPosition;
    private bool isPinchTracking = false;
    private float minSwipeDistance = 0.05f;
    
    // Distance maximale pour la sélection d'un cube
    private float maxSelectionDistance = 0.15f;
    
    private bool isDebugEnabled = true;
    
    // Pour stocker le cube sélectionné
    private GameObject selectedCube = null;

    private void Start()
    {
        rubikGen = GetComponent<RubikGen>();
        if (isDebugEnabled) Debug.Log("RubikRotation started");
    }

    private void Update()
    {
        if (rightHand == null || leftHand == null)
        {
            if (isDebugEnabled) Debug.LogWarning("Hands references missing");
            return;
        }

        if (isRotating) return;

        HandleHandInteraction(rightHand, rightHandSkeleton, ref wasRightPinching, true);
        HandleHandInteraction(leftHand, leftHandSkeleton, ref wasLeftPinching, false);
    }

    private void HandleHandInteraction(OVRHand hand, OVRSkeleton skeleton, ref bool wasPinching, bool isRightHand)
    {
        if (hand == null || skeleton == null) return;

        bool isPinching = IsHandPinching(hand);
        Vector3 fingerPosition = GetFingerTipPosition(skeleton);

        if (isPinching && !wasPinching && !isRotating)
        {
            // Au début du pincement
            selectedCube = FindClosestCube(fingerPosition);
            if (selectedCube != null)
            {
                pinchStartPosition = fingerPosition;
                isPinchTracking = true;
                if (isDebugEnabled) Debug.Log($"Selected cube: {selectedCube.name}");
            }
        }
        else if (isPinching && isPinchTracking && selectedCube != null)
        {
            // Pendant le pincement
            Vector3 movement = fingerPosition - pinchStartPosition;
            
            if (movement.magnitude >= minSwipeDistance)
            {
                Vector3Int coords = GetCubeCoordinates(selectedCube);
                Vector3 localMovement = transform.InverseTransformDirection(movement);
                
                if (TryGetRotationFromMovement(coords, localMovement, out RotationCommand cmd))
                {
                    StartCoroutine(ExecuteRotation(cmd));
                    selectedCube = null;
                    isPinchTracking = false;
                }
            }
        }
        else if (!isPinching && wasPinching)
        {
            // Fin du pincement
            selectedCube = null;
            isPinchTracking = false;
        }

        wasPinching = isPinching;
    }

    private struct RotationCommand
    {
        public int slice;         // Index de la tranche
        public Vector3 axis;      // Axe de rotation
        public bool clockwise;    // Direction
        public SliceType type;    // Type de tranche (X, Y, ou Z)
    }

    private enum SliceType
    {
        X, Y, Z
    }

    private GameObject FindClosestCube(Vector3 position)
    {
        GameObject closest = null;
        float minDistance = maxSelectionDistance;

        // Convertir la position en coordonnées locales du Rubik's Cube
        Vector3 localPosition = transform.InverseTransformPoint(position);

        for (int x = 0; x < rubikGen.cubeSize; x++)
        {
            for (int y = 0; y < rubikGen.cubeSize; y++)
            {
                for (int z = 0; z < rubikGen.cubeSize; z++)
                {
                    GameObject cube = rubikGen.cubeArray[x, y, z];
                    if (cube != null)
                    {
                        Vector3 cubeLocalPos = transform.InverseTransformPoint(cube.transform.position);
                        float distance = Vector3.Distance(localPosition, cubeLocalPos);
                        
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closest = cube;
                        }
                    }
                }
            }
        }

        return closest;
    }

    private Vector3Int GetCubeCoordinates(GameObject cube)
    {
        string[] coords = cube.name.Substring(4).Split('_');
        return new Vector3Int(
            int.Parse(coords[0][0].ToString()),
            int.Parse(coords[0][1].ToString()),
            int.Parse(coords[1])
        );
    }

    private bool TryGetRotationFromMovement(Vector3Int cubeCoords, Vector3 movement, out RotationCommand cmd)
    {
        cmd = new RotationCommand();
        
        // Position relative du cube par rapport au centre du Rubik's Cube
        Vector3 relativePosition = new Vector3(
            cubeCoords.x - (rubikGen.cubeSize - 1) / 2f,
            cubeCoords.y - (rubikGen.cubeSize - 1) / 2f,
            cubeCoords.z - (rubikGen.cubeSize - 1) / 2f
        );

        // Convertir le mouvement en coordonnées locales
        Vector3 localMovement = movement.normalized;

        // Déterminer quelle face est la plus proche du cube sélectionné
        int dominantAxis = GetDominantAxis(relativePosition);
        
        if (isDebugEnabled)
        {
            Debug.Log($"Relative position: {relativePosition}, Dominant axis: {dominantAxis}, Movement: {localMovement}");
        }

        // Déterminer la rotation en fonction de la face et du mouvement
        switch (dominantAxis)
        {
            case 0: // Face X (droite/gauche)
                if (Mathf.Abs(localMovement.y) > Mathf.Abs(localMovement.z))
                {
                    // Mouvement vertical -> rotation autour de Z
                    cmd.type = SliceType.Z;
                    cmd.slice = cubeCoords.z;
                    cmd.axis = Vector3.forward;
                    cmd.clockwise = localMovement.y * Mathf.Sign(relativePosition.x) > 0;
                }
                else
                {
                    // Mouvement horizontal -> rotation autour de Y
                    cmd.type = SliceType.Y;
                    cmd.slice = cubeCoords.y;
                    cmd.axis = Vector3.up;
                    cmd.clockwise = localMovement.z * Mathf.Sign(relativePosition.x) < 0;
                }
                break;

            case 1: // Face Y (haut/bas)
                if (Mathf.Abs(localMovement.x) > Mathf.Abs(localMovement.z))
                {
                    // Mouvement horizontal -> rotation autour de Z
                    cmd.type = SliceType.Z;
                    cmd.slice = cubeCoords.z;
                    cmd.axis = Vector3.forward;
                    cmd.clockwise = localMovement.x * Mathf.Sign(relativePosition.y) < 0;
                }
                else
                {
                    // Mouvement en profondeur -> rotation autour de X
                    cmd.type = SliceType.X;
                    cmd.slice = cubeCoords.x;
                    cmd.axis = Vector3.right;
                    cmd.clockwise = localMovement.z * Mathf.Sign(relativePosition.y) > 0;
                }
                break;

            case 2: // Face Z (avant/arrière)
                if (Mathf.Abs(localMovement.x) > Mathf.Abs(localMovement.y))
                {
                    // Mouvement horizontal -> rotation autour de Y
                    cmd.type = SliceType.Y;
                    cmd.slice = cubeCoords.y;
                    cmd.axis = Vector3.up;
                    cmd.clockwise = localMovement.x * Mathf.Sign(relativePosition.z) > 0;
                }
                else
                {
                    // Mouvement vertical -> rotation autour de X
                    cmd.type = SliceType.X;
                    cmd.slice = cubeCoords.x;
                    cmd.axis = Vector3.right;
                    cmd.clockwise = localMovement.y * Mathf.Sign(relativePosition.z) < 0;
                }
                break;
        }

        if (isDebugEnabled)
        {
            Debug.Log($"Rotation command: Type={cmd.type}, Slice={cmd.slice}, Axis={cmd.axis}, Clockwise={cmd.clockwise}");
        }

        return true;
    }

    private int GetDominantAxis(Vector3 position)
    {
        float absX = Mathf.Abs(position.x);
        float absY = Mathf.Abs(position.y);
        float absZ = Mathf.Abs(position.z);

        if (absX > absY || absX > absZ) return 0; // X est dominant
        if (absY > absX || absY > absZ) return 1; // Y est dominant
        return 2; // Z est dominant
    }

    private IEnumerator ExecuteRotation(RotationCommand cmd)
    {
        if (isRotating) yield break;
        isRotating = true;

        GameObject rotationParent = new GameObject("RotationParent");
        rotationParent.transform.position = transform.position;
        rotationParent.transform.rotation = transform.rotation;

        // Collecter les cubes de la tranche
        List<GameObject> sliceCubes = new List<GameObject>();

        switch (cmd.type)
        {
            case SliceType.Y:
                for (int x = 0; x < rubikGen.cubeSize; x++)
                    for (int z = 0; z < rubikGen.cubeSize; z++)
                        if (rubikGen.cubeArray[x, cmd.slice, z] != null)
                            sliceCubes.Add(rubikGen.cubeArray[x, cmd.slice, z]);
                break;

            case SliceType.X:
                for (int y = 0; y < rubikGen.cubeSize; y++)
                    for (int z = 0; z < rubikGen.cubeSize; z++)
                        if (rubikGen.cubeArray[cmd.slice, y, z] != null)
                            sliceCubes.Add(rubikGen.cubeArray[cmd.slice, y, z]);
                break;
                
            case SliceType.Z:
                for (int x = 0; x < rubikGen.cubeSize; x++)
                    for (int y = 0; y < rubikGen.cubeSize; y++)
                        if (rubikGen.cubeArray[x, y, cmd.slice] != null)
                            sliceCubes.Add(rubikGen.cubeArray[x, y, cmd.slice]);
                break;
        }

        // Parenter les cubes
        foreach (GameObject cube in sliceCubes)
        {
            cube.transform.parent = rotationParent.transform;
        }

        // Effectuer la rotation
        float angle = 0;
        while (angle < 90f)
        {
            float rotation = rotationSpeed * Time.deltaTime;
            angle += rotation;
            rotationParent.transform.Rotate(cmd.axis * (cmd.clockwise ? rotation : -rotation));
            yield return null;
        }

        // Ajuster à exactement 90 degrés
        float adjustment = 90f - angle;
        rotationParent.transform.Rotate(cmd.axis * (cmd.clockwise ? adjustment : -adjustment));

        // Reparenter les cubes et mettre à jour leurs positions
        foreach (GameObject cube in sliceCubes)
        {
            cube.transform.parent = transform;
            UpdateCubeArrayPosition(cube);
        }

        Destroy(rotationParent);
        isRotating = false;

        yield return new WaitForSeconds(0.1f);
    }

    private Vector3 GetFingerTipPosition(OVRSkeleton skeleton) => 
        skeleton.Bones[(int)OVRPlugin.BoneId.Hand_IndexTip].Transform.position;

    private bool IsHandPinching(OVRHand hand) => 
        hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > pinchThreshold;

    private void UpdateCubeArrayPosition(GameObject cube)
    {
        try
        {
            string[] coords = cube.name.Substring(4).Split('_');
            int originalX = int.Parse(coords[0][0].ToString());
            int originalY = int.Parse(coords[0][1].ToString());
            int originalZ = int.Parse(coords[1]);

            Vector3 localPos = transform.InverseTransformPoint(cube.transform.position);
            int newX = Mathf.RoundToInt(localPos.x / rubikGen.offset + (rubikGen.cubeSize - 1) / 2f);
            int newY = Mathf.RoundToInt(localPos.y / rubikGen.offset + (rubikGen.cubeSize - 1) / 2f);
            int newZ = Mathf.RoundToInt(localPos.z / rubikGen.offset + (rubikGen.cubeSize - 1) / 2f);

            newX = Mathf.Clamp(newX, 0, rubikGen.cubeSize - 1);
            newY = Mathf.Clamp(newY, 0, rubikGen.cubeSize - 1);
            newZ = Mathf.Clamp(newZ, 0, rubikGen.cubeSize - 1);

            if (rubikGen.cubeArray[originalX, originalY, originalZ] == cube)
            {
                rubikGen.cubeArray[originalX, originalY, originalZ] = null;
            }
            rubikGen.cubeArray[newX, newY, newZ] = cube;
            cube.name = $"Cube{newX}{newY}_{newZ}";
        }
        catch (System.Exception e)
        {
            if (isDebugEnabled) Debug.LogError($"Error updating cube position: {e.Message}");
        }
    }
}
