using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contrôle le mouvement d'un GameObject vers des points spécifiques (EmptyObjects) dans la scène.
/// Gère la rotation du GameObject en fonction de son mouvement et de la surface sur laquelle il se trouve.
/// </summary>
public class MovementController : MonoBehaviour
{
    // Paramètres publics configurables dans l'inspecteur
    public float detectionRadius = 5f;      // Rayon de détection des EmptyObjects
    public float speed = 4f;                // Vitesse de déplacement
    public float rotationSpeed = 10f;       // Vitesse de rotation
    public float tileDetectionRadius = 0.5f; // Rayon de détection des tuiles
    
    // Variables privées pour la gestion interne
    private Transform target;               // Cible actuelle vers laquelle se déplacer
    private List<Transform> potentialTargets = new List<Transform>();  // Liste des cibles potentielles
    private bool isWaitingForInput = false; // Indique si on attend une entrée utilisateur
    private Transform currentTile;          // Tuile actuelle sur laquelle se trouve le GameObject
    private Quaternion targetRotation;      // Rotation cible pour l'alignement avec la tuile
    private bool isRotating = false;        // Indique si le GameObject est en train de tourner
    private Vector3 lastPosition;           // Dernière position pour calculer la direction du mouvement
    private bool isMoving = false;          // Indique si le GameObject est en mouvement

    /// <summary>
    /// Initialisation des variables au démarrage
    /// </summary>
    void Start()
    {
        lastPosition = transform.position;
    }

    /// <summary>
    /// Mise à jour appelée à chaque frame
    /// Gère la logique principale du mouvement et de la rotation
    /// </summary>
    void Update()
    {
        CheckCurrentTile();

        // Recherche une nouvelle cible si nécessaire
        if (target == null)
        {
            FindNearestEmptyObjects();
        }

        // Gestion des inputs si plusieurs chemins sont possibles
        if (isWaitingForInput)
        {
            Debug.Log("Nombre de cibles à proximité: " + potentialTargets.Count);
            HandleUserInput();
        }

        // Gestion du mouvement et de la rotation si une cible est définie
        if (target != null && !isWaitingForInput)
        {
            if (isRotating)
            {
                UpdateRotation();
            }
            
            MoveTowardsTarget();
            UpdateMovementRotation();
        }

        lastPosition = transform.position;
    }

    /// <summary>
    /// Met à jour la rotation du GameObject en fonction de sa direction de déplacement
    /// </summary>
    void UpdateMovementRotation()
    {
        Vector3 movementDirection = transform.position - lastPosition;
        
        if (movementDirection.magnitude > 0.001f)
        {
            isMoving = true;
            
            // Calcul de la rotation en fonction de la direction et de la normale de la tuile
            Quaternion movementRotation = Quaternion.LookRotation(
                movementDirection.normalized, 
                currentTile ? currentTile.up : Vector3.up
            );
            
            // Application progressive de la rotation
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                movementRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            isMoving = false;
        }
    }

    /// <summary>
    /// Vérifie et met à jour la tuile sur laquelle se trouve le GameObject
    /// </summary>
    void CheckCurrentTile()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, tileDetectionRadius);
        Transform closestTile = null;
        float closestDistance = float.MaxValue;

        // Recherche de la tuile la plus proche
        foreach (Collider collider in nearbyColliders)
        {
            if (collider.CompareTag("Tuile"))
            {
                Vector3 tileCenter = collider.bounds.center;
                float distance = Vector3.Distance(transform.position, tileCenter);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTile = collider.transform;
                }
            }
        }

        // Mise à jour de l'alignement si nouvelle tuile
        if (closestTile != null && closestTile != currentTile)
        {
            currentTile = closestTile;
            AlignWithTile(currentTile);
        }
    }

    /// <summary>
    /// Aligne le GameObject avec la normale de la tuile
    /// </summary>
    /// <param name="tile">La tuile avec laquelle s'aligner</param>
    void AlignWithTile(Transform tile)
    {
        if (!isMoving)
        {
            Vector3 tileNormal = tile.up;
            Quaternion newRotation = Quaternion.FromToRotation(Vector3.up, tileNormal);
            
            float currentYRotation = transform.rotation.eulerAngles.y;
            Vector3 targetEuler = newRotation.eulerAngles;
            targetRotation = Quaternion.Euler(targetEuler.x, currentYRotation, targetEuler.z);
            
            isRotating = true;
        }
    }

    /// <summary>
    /// Met à jour la rotation progressive vers la rotation cible
    /// </summary>
    void UpdateRotation()
    {
        if (!isMoving)
        {
            float angle = Quaternion.Angle(transform.rotation, targetRotation);
            
            if (angle > 0.1f)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    /// <summary>
    /// Recherche les EmptyObjects les plus proches dans le rayon de détection
    /// </summary>
    void FindNearestEmptyObjects()
    {
        GameObject[] emptyObjects = GameObject.FindGameObjectsWithTag("Point");
        potentialTargets.Clear();
        float minDistance = Mathf.Infinity;

        // Recherche des points à égale distance
        foreach (GameObject emptyObject in emptyObjects)
        {
            float distance = Vector3.Distance(transform.position, emptyObject.transform.position);

            if (distance < minDistance && distance <= detectionRadius)
            {
                minDistance = distance;
                potentialTargets.Clear();
                potentialTargets.Add(emptyObject.transform);
            }
            else if (Mathf.Abs(distance - minDistance) < 0.01f)
            {
                potentialTargets.Add(emptyObject.transform);
            }
        }

        // Gestion des chemins multiples
        if (potentialTargets.Count > 1)
        {
            isWaitingForInput = true;
            target = null;
        }
        else if (potentialTargets.Count == 1)
        {
            target = potentialTargets[0];
        }
    }

    /// <summary>
    /// Gère les entrées utilisateur pour la sélection des chemins
    /// </summary>
    void HandleUserInput()
    {
        if (potentialTargets.Count == 0) return;

        // Sélection de la cible en fonction de la touche pressée
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            target = potentialTargets[0];
            isWaitingForInput = false;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && potentialTargets.Count > 1)
        {
            target = potentialTargets[1];
            isWaitingForInput = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && potentialTargets.Count > 2)
        {
            target = potentialTargets[2];
            isWaitingForInput = false;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && potentialTargets.Count > 3)
        {
            target = potentialTargets[3];
            isWaitingForInput = false;
        }
    }

    /// <summary>
    /// Déplace le GameObject vers la cible actuelle
    /// </summary>
    void MoveTowardsTarget()
    {
        if (target == null) return;

        // Déplacement vers la cible
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Vérification si la cible est atteinte
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            transform.position = target.position;
            target.gameObject.SetActive(false);
            target = null;
        }
    }

    /// <summary>
    /// Dessine les Gizmos dans l'éditeur pour visualiser le rayon de détection
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}