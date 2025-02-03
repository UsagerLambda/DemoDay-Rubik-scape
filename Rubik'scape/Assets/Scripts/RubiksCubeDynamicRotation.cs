using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RubiksCubeInteraction : MonoBehaviour
{
    [Header("Références")]
    public XRDirectInteractor leftHand; // Main gauche (pour tourner une section)
    public XRDirectInteractor rightHand; // Main droite (pour grab complet)
    public Transform rubikGen; // Objet parent du Rubik’s Cube

    [Header("Paramètres de rotation")]
    public float rotationSpeed = 50f; // Vitesse de rotation des sections
    public float snapAngleThreshold = 30f; // Seuil d'alignement après rotation

    private GameObject selectedCube = null; // Cube actuellement sélectionné
    private bool isRotating = false; // Empêche plusieurs rotations simultanées
    private Vector3 rotationAxis; // Axe de rotation sélectionné
    private Transform[] selectedLayer; // Liste des cubes de la couche à tourner

    void Start()
    {
        if (leftHand != null)
            leftHand.selectEntered.AddListener(OnLeftHandGrab);
        
        if (rightHand != null)
            rightHand.selectEntered.AddListener(OnRightHandGrab);
    }

    // Gestion du grab avec la main droite (Grab normal avec XR Toolkit)
    private void OnRightHandGrab(SelectEnterEventArgs args)
    {
        // XR Grab Interactable fait déjà le travail, rien à gérer ici.
        Debug.Log("Grab du cube entier avec la main droite.");
    }

    // Gestion du grab avec la main gauche (Rotation d'une section)
    private void OnLeftHandGrab(SelectEnterEventArgs args)
    {
        if (isRotating) return; // Évite une rotation multiple

        selectedCube = args.interactableObject.transform.gameObject;
        rotationAxis = DetermineRotationAxis(selectedCube);
        selectedLayer = GetLayerCubes(selectedCube, rotationAxis);

        if (selectedLayer != null && selectedLayer.Length > 0)
        {
            Debug.Log($"Début rotation axe: {rotationAxis}, {selectedLayer.Length} cubes affectés.");
            StartCoroutine(RotateLayer(selectedLayer, rotationAxis));
        }
    }

    // Détermine l'axe de rotation en fonction du cube sélectionné
    private Vector3 DetermineRotationAxis(GameObject cube)
    {
        Vector3 localPosition = rubikGen.InverseTransformPoint(cube.transform.position);
        if (Mathf.Abs(localPosition.x) > Mathf.Abs(localPosition.y) && Mathf.Abs(localPosition.x) > Mathf.Abs(localPosition.z))
            return Vector3.right;
        if (Mathf.Abs(localPosition.y) > Mathf.Abs(localPosition.x) && Mathf.Abs(localPosition.y) > Mathf.Abs(localPosition.z))
            return Vector3.up;
        return Vector3.forward;
    }

    // Récupère les cubes de la même section à tourner
    private Transform[] GetLayerCubes(GameObject cube, Vector3 axis)
    {
        List<Transform> layerCubes = new List<Transform>();
        foreach (Transform child in rubikGen)
        {
            Vector3 localPos = rubikGen.InverseTransformPoint(child.position);
            Vector3 refPos = rubikGen.InverseTransformPoint(cube.transform.position);

            if (axis == Vector3.right && Mathf.Abs(localPos.x - refPos.x) < 0.1f ||
                axis == Vector3.up && Mathf.Abs(localPos.y - refPos.y) < 0.1f ||
                axis == Vector3.forward && Mathf.Abs(localPos.z - refPos.z) < 0.1f)
            {
                layerCubes.Add(child);
            }
        }
        return layerCubes.ToArray();
    }

    // Rotation fluide de la section sélectionnée
    private System.Collections.IEnumerator RotateLayer(Transform[] cubes, Vector3 axis)
    {
        isRotating = true;
        Quaternion startRotation = rubikGen.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(90, axis) * startRotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * (rotationSpeed / 100);
            rubikGen.rotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        rubikGen.rotation = endRotation;
        SnapLayer(cubes, axis);
        isRotating = false;
    }

    // Ajuste l'angle final pour bien s'aligner
    private void SnapLayer(Transform[] cubes, Vector3 axis)
    {
        float angle = Vector3.SignedAngle(Vector3.up, rubikGen.up, axis);
        float snappedAngle = Mathf.Round(angle / snapAngleThreshold) * snapAngleThreshold;
        rubikGen.rotation = Quaternion.AngleAxis(snappedAngle, axis);
    }
}
