using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Ce script gère deux types d'interactions :
/// - La main droite agit sur l’objet parent (grab complet) via XRGrabInteractable.
/// - La main gauche permet de saisir une pièce (tag "CubePiece") pour faire tourner la section correspondante.
/// 
/// ATTENTION : Ce script doit être attaché à l’objet parent du Rubik’s Cube.  
/// L’objet parent doit posséder le composant XRGrabInteractable pour le grab global (main droite).
/// </summary>
public class RubiksCubeInteraction : MonoBehaviour
{
    [Header("Références Interactives")]
    [Tooltip("XRDirectInteractor correspondant à la main gauche (utilisé pour la rotation d'une section).")]
    public XRDirectInteractor leftHand;
    [Tooltip("XRDirectInteractor correspondant à la main droite (pour information, le grab global se fait sur l'objet parent).")]
    public XRDirectInteractor rightHand;
    [Tooltip("Composant XRGrabInteractable attaché à cet objet (pour le grab global par la main droite).")]
    public XRGrabInteractable parentGrabInteractable;

    [Header("Paramètres de rotation (Main Gauche)")]
    [Tooltip("Vitesse de rotation appliquée à la section du Rubik's Cube.")]
    public float rotationSpeed = 100f;
    [Tooltip("Tolérance pour la sélection de la couche (en unités locales).")]
    public float layerThreshold = 0.2f;

    // Variables internes pour la rotation par la main gauche
    private bool isRotating = false;           // Indique qu'une rotation est en cours
    private GameObject selectedCubePiece;      // La pièce saisie par la main gauche
    private Vector3 rotationAxis;              // Axe de rotation déterminé à partir de la pièce saisie
    private List<Transform> selectedLayer;     // Liste des cubes appartenant à la même couche
    private Vector3 previousLeftHandPos;       // Dernière position connue de la main gauche

    void Start()
    {
        // Si la référence au parentGrabInteractable n'est pas assignée, on tente de la récupérer sur cet objet
        if (parentGrabInteractable == null)
            parentGrabInteractable = GetComponent<XRGrabInteractable>();

        // Abonnement aux événements du XRDirectInteractor de la main gauche
        if (leftHand != null)
        {
            leftHand.selectEntered.AddListener(OnLeftHandGrab);
            leftHand.selectExited.AddListener(OnLeftHandRelease);
        }
        else
        {
            Debug.LogError("Left Hand (XRDirectInteractor) non assigné dans RubiksCubeInteraction.");
        }
    }

    /// <summary>
    /// Lorsqu'une pièce est saisie par la main gauche, on démarre le mode rotation.
    /// On désactive temporairement le XRGrabInteractable global pour éviter les conflits.
    /// </summary>
    private void OnLeftHandGrab(SelectEnterEventArgs args)
    {
        // On ne gère que les pièces ayant le tag "CubePiece"
        GameObject grabbed = args.interactableObject.transform.gameObject;
        if (grabbed.CompareTag("CubePiece"))
        {
            isRotating = true;
            selectedCubePiece = grabbed;
            previousLeftHandPos = leftHand.transform.position;

            // Désactivation du grab global pendant la rotation
            if (parentGrabInteractable != null)
                parentGrabInteractable.enabled = false;

            // Détermine l'axe de rotation en fonction de la position locale de la pièce saisie
            rotationAxis = DetermineRotationAxis(grabbed);
            // Récupère la liste des cubes appartenant à la même couche (section)
            selectedLayer = GetLayerCubes(grabbed, rotationAxis);
        }
    }

    /// <summary>
    /// Lorsque la main gauche relâche la pièce, on termine la rotation.
    /// Le snap (alignement) peut être appliqué ici si nécessaire.
    /// On réactive ensuite le grab global.
    /// </summary>
    private void OnLeftHandRelease(SelectExitEventArgs args)
    {
        if (isRotating)
        {
            // Optionnel : ajouter ici un appel à une méthode de "snap" pour aligner précisément la rotation.
            // Par exemple : SnapRotation();

            isRotating = false;
            selectedCubePiece = null;
            if (selectedLayer != null)
                selectedLayer.Clear();

            // Réactivation du grab global sur l'objet parent
            if (parentGrabInteractable != null)
                parentGrabInteractable.enabled = true;
        }
    }

    void Update()
    {
        // Si une rotation est en cours par la main gauche, on la met à jour en fonction du mouvement de la main
        if (isRotating)
        {
            Vector3 currentPos = leftHand.transform.position;
            Vector3 delta = currentPos - previousLeftHandPos;
            // Projection du déplacement sur le plan perpendiculaire à l'axe de rotation
            Vector3 projected = Vector3.ProjectOnPlane(delta, rotationAxis);
            // Calcul de l'angle de rotation (en degrés)
            float angle = projected.magnitude * rotationSpeed;
            // Détermination du signe de rotation à l'aide du produit vectoriel
            float sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(rotationAxis, projected), Vector3.up));
            angle *= sign;

            // Applique la rotation à tous les cubes de la couche sélectionnée
            if (selectedLayer != null)
            {
                foreach (Transform cube in selectedLayer)
                {
                    // La rotation se fait autour du centre de l'objet parent
                    cube.RotateAround(transform.position, rotationAxis, angle);
                }
            }
            previousLeftHandPos = currentPos;
        }
    }

    /// <summary>
    /// Détermine l'axe de rotation en regardant la position locale de la pièce saisie.
    /// On choisit l'axe dont la composante (x, y ou z) est la plus importante en valeur absolue.
    /// </summary>
    /// <param name="cube">La pièce saisie</param>
    /// <returns>L'axe de rotation (Vector3.right, Vector3.up ou Vector3.forward)</returns>
    private Vector3 DetermineRotationAxis(GameObject cube)
    {
        Vector3 localPos = transform.InverseTransformPoint(cube.transform.position);
        float absX = Mathf.Abs(localPos.x);
        float absY = Mathf.Abs(localPos.y);
        float absZ = Mathf.Abs(localPos.z);

        if (absX >= absY && absX >= absZ)
            return Vector3.right;
        else if (absY >= absX && absY >= absZ)
            return Vector3.up;
        else
            return Vector3.forward;
    }

    /// <summary>
    /// Récupère la liste des cubes (enfants de l'objet parent) qui appartiennent à la même couche
    /// que la pièce saisie, en se basant sur l'axe de rotation.
    /// La tolérance layerThreshold permet de regrouper les pièces proches.
    /// </summary>
    /// <param name="cube">La pièce saisie</param>
    /// <param name="axis">L'axe de rotation déterminé</param>
    /// <returns>Liste des Transforms des cubes de la couche</returns>
    private List<Transform> GetLayerCubes(GameObject cube, Vector3 axis)
    {
        List<Transform> layer = new List<Transform>();
        Vector3 refLocal = transform.InverseTransformPoint(cube.transform.position);

        foreach (Transform child in transform)
        {
            Vector3 childLocal = transform.InverseTransformPoint(child.position);
            if (axis == Vector3.right)
            {
                if (Mathf.Abs(childLocal.x - refLocal.x) < layerThreshold)
                    layer.Add(child);
            }
            else if (axis == Vector3.up)
            {
                if (Mathf.Abs(childLocal.y - refLocal.y) < layerThreshold)
                    layer.Add(child);
            }
            else if (axis == Vector3.forward)
            {
                if (Mathf.Abs(childLocal.z - refLocal.z) < layerThreshold)
                    layer.Add(child);
            }
        }
        return layer;
    }

    /*
    /// <summary>
    /// Méthode optionnelle pour réaligner (snap) la rotation après le relâchement de la main gauche.
    /// Vous pouvez implémenter ici une logique pour arrondir l'angle de rotation au multiple de 90°.
    /// </summary>
    private void SnapRotation()
    {
        // Exemple d'implémentation à adapter selon vos besoins...
    }
    */
}
