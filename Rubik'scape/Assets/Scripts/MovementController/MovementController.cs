using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class MovementController : MonoBehaviour {
    [Header("Movement Settings")]
    public float detectionRadius = 0.05f;
    public float speed = 4f;
    public float rotationSpeed = 10f;
    public float tileDetectionRadius = 0.5f;
    public float rayDistance = 2.5f;

    [Header("UI References")]
    public Button frontArrow;
    public Button leftArrow;
    public Button rightArrow;

    [Header("Collision Boxes")]
    public BoxCollider frontCollider;
    public BoxCollider leftCollider;
    public BoxCollider rightCollider;

    private Transform target;
    private List<Transform> availablePoints = new List<Transform>();
    private Transform currentTile;
    private Quaternion targetRotation;
    private Vector3 lastPosition;

    private bool isWaitingForInput;
    private bool isRotating;
    private bool isMoving;
    private bool isInitialized;

    private void Awake() {
        Debug.Log("Vérification des boutons :");
        Debug.Log("Front Arrow : " + (frontArrow != null));
        Debug.Log("Left Arrow : " + (leftArrow != null));
        Debug.Log("Right Arrow : " + (rightArrow != null));

        InitializeButtonListeners();
        SetButtonsActive(false);
    }

    private void InitializeButtonListeners() {
        if (frontArrow) {
            frontArrow.onClick.AddListener(() => {
                Debug.Log("Clic sur bouton avant");
                HandleDirectionButton(frontCollider);
            });
        }
        if (leftArrow) {
            leftArrow.onClick.AddListener(() => {
                Debug.Log("Clic sur bouton gauche");
                HandleDirectionButton(leftCollider);
            });
        }
        if (rightArrow) {
            rightArrow.onClick.AddListener(() => {
                Debug.Log("Clic sur bouton droit");
                HandleDirectionButton(rightCollider);
            });
        }
        isInitialized = true;
    }

    private void HandleDirectionButton(BoxCollider directionCollider) {
    if (!isWaitingForInput || directionCollider == null) return;

    // Cherche et désactive le point Multi où se trouve le personnage
    var multiPoint = availablePoints.FirstOrDefault(p => 
        p.CompareTag("Multi") && 
        Vector3.Distance(transform.position, p.position) < 0.1f
    );
    
    if (multiPoint != null) {
        Debug.Log("Désactivation du point Multi");
        multiPoint.gameObject.SetActive(false);
    }

    // Désactive les boutons
    isWaitingForInput = false;
    SetButtonsActive(false);

    // Fait avancer légèrement le personnage dans la direction du collider
    Vector3 direction = (directionCollider.transform.position - transform.position).normalized;
    transform.position += direction * 0.015f; // Déplacement de 60% de la distance entre points

    // On laisse la logique existante prendre le relais
    target = null;
    FindNextTarget();
}

    private System.Collections.IEnumerator DestroyTargetWhenReached(GameObject tempTarget) {
        // Attend que le personnage soit très proche de la cible
        while (target != null && Vector3.Distance(transform.position, tempTarget.transform.position) > 0.1f) {
            yield return null;
        }
        
        // Une fois arrivé, on détruit la cible temporaire
        if (tempTarget != null) {
            target = null;
            Destroy(tempTarget);
        }
    }

    private void ShowAvailableDirections() {
        SetButtonsActive(false);

        // Vérifier chaque collider pour la présence de points
        CheckDirectionForPoint(frontCollider, frontArrow);
        CheckDirectionForPoint(leftCollider, leftArrow);
        CheckDirectionForPoint(rightCollider, rightArrow);
    }

    private void CheckDirectionForPoint(BoxCollider collider, Button button) {
        if (!collider || !button) return;

        // Vérifier s'il y a un point dans cette direction
        Collider[] hitColliders = Physics.OverlapBox(
            collider.transform.position,
            collider.bounds.extents,
            collider.transform.rotation
        );

        foreach (var hitCollider in hitColliders) {
            if (hitCollider.CompareTag("Point") || hitCollider.CompareTag("Multi")) {
                button.gameObject.SetActive(true);
                break;
            }
        }
    }

    // Les autres méthodes restent identiques à votre code original
    // (OnEnable, OnDisable, OnDestroy, Update, CheckCurrentTile, etc.)
    private void OnEnable() {
        ResetState();
        GatherAllPoints();
        FindNextTarget();
    }

    private void OnDisable() {
        ResetState();
        SetButtonsActive(false);
    }

    private void OnDestroy() => RemoveButtonListeners();

    private void Update() {
        if (!isInitialized) {
            InitializeButtonListeners();
            return;
        }

        CheckCurrentTile();

        if (target == null && !isWaitingForInput) {
            FindNextTarget();
        }

        if (!isWaitingForInput && target != null) {
            if (isRotating) UpdateRotation();
            MoveTowardsTarget();
            UpdateMovementRotation();
        }
        lastPosition = transform.position;
    }

    private void RemoveButtonListeners() {
        if (frontArrow) frontArrow.onClick.RemoveAllListeners();
        if (leftArrow) leftArrow.onClick.RemoveAllListeners();
        if (rightArrow) rightArrow.onClick.RemoveAllListeners();
        isInitialized = false;
    }

    private void ResetState() {
        lastPosition = transform.position;
        target = null;
        isWaitingForInput = false;
        isRotating = false;
        isMoving = false;
        currentTile = null;
        targetRotation = transform.rotation;
        availablePoints.Clear();
        SetButtonsActive(false);
    }

    private void CheckCurrentTile() {
        var nearbyColliders = Physics.OverlapSphere(transform.position, tileDetectionRadius);
        Transform closestTile = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in nearbyColliders) {
            if (collider.CompareTag("Tuile")) {
                float distance = Vector3.Distance(transform.position, collider.bounds.center);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestTile = collider.transform;
                }
            }
        }
        if (closestTile != null && closestTile != currentTile) {
            currentTile = closestTile;
            AlignWithTile(currentTile);
        }
    }

    private void AlignWithTile(Transform tile) {
        if (!isMoving) {
            Vector3 targetEuler = Quaternion.FromToRotation(Vector3.up, tile.up).eulerAngles;
            targetRotation = Quaternion.Euler(targetEuler.x, transform.rotation.eulerAngles.y, targetEuler.z);
            isRotating = true;
        }
    }

    private void UpdateRotation() {
        if (!isMoving) {
            float angle = Quaternion.Angle(transform.rotation, targetRotation);
            if (angle > 0.1f) {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            } else {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    private void GatherAllPoints() {
        availablePoints.Clear();
        foreach (var tag in new[] { "Point", "Multi" }) {
            var points = GameObject.FindGameObjectsWithTag(tag);
            foreach (var point in points) {
                availablePoints.Add(point.transform);
            }
        }
    }

    private void FindNextTarget() {
        if (availablePoints.Count == 0) return;

        float minDistance = float.MaxValue;
        Transform closestPoint = null;

        foreach (var point in availablePoints) {
            if (!point.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(transform.position, point.position);
            if (distance < minDistance && distance <= detectionRadius) {
                minDistance = distance;
                closestPoint = point;
            }
        }
        target = closestPoint;
    }

    private void HandleUserInput() {
        if (availablePoints.Count == 0) return;
        ShowAvailableDirections();
    }

    private void SetButtonsActive(bool active) {
        if (frontArrow) frontArrow.gameObject.SetActive(active);
        if (leftArrow) leftArrow.gameObject.SetActive(active);
        if (rightArrow) rightArrow.gameObject.SetActive(active);
    }

    private void MoveTowardsTarget() {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        Debug.Log($"Current rotation: {transform.rotation.eulerAngles}, Moving to target: {target.name}");

        if (Vector3.Distance(transform.position, target.position) < 0.1f) {
            transform.position = target.position;

            if (target.CompareTag("Multi")) {
                target.gameObject.SetActive(true);
                isWaitingForInput = true;
                HandleUserInput();
            } else {
                if (currentTile && currentTile.CompareTag("Multi")) {
                    Debug.Log("Désactivation du point Multi");
                    currentTile.gameObject.SetActive(false);
                }
                target.gameObject.SetActive(false);
                availablePoints.Remove(target);
                target = null;
                FindNextTarget();
            }
        }
    }

    private void UpdateMovementRotation() {
        Vector3 movementDirection = transform.position - lastPosition;
        if (movementDirection.magnitude > 0.001f) {
            isMoving = true;
            Vector3 upDirection = currentTile ? currentTile.up : Vector3.up;

            Quaternion targetRotation = Quaternion.LookRotation(movementDirection.normalized, upDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime * 100f
            );
        } else {
            isMoving = false;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.DrawWireSphere(transform.position, rayDistance);
    }
}

