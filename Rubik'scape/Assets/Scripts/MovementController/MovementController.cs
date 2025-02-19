using UnityEngine;
using System.Collections.Generic;

public class MovementController : MonoBehaviour
{
    public float detectionRadius = 5f;
    public float speed = 4f;
    private Transform target;
    private List<Transform> potentialTargets = new List<Transform>();
    private bool isWaitingForInput = false;

    void Update()
    {
        // Si on n'a pas de cible, chercher le point le plus proche
        if (target == null)
        {
            FindNearestPoint();
        }

        // Si on a une cible et qu'on n'attend pas d'input, on bouge
        if (target != null && !isWaitingForInput)
        {
            MoveTowardsTarget();
        }
        // Si on attend un input, gérer les touches
        else if (isWaitingForInput)
        {
            HandleUserInput();
        }
    }

    void FindNearestPoint()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("Point");
        potentialTargets.Clear();
        
        if (points.Length == 0) return;

        // Trouver le point le plus proche
        Transform nearestPoint = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject point in points)
        {
            float distance = Vector3.Distance(transform.position, point.transform.position);
            if (distance <= detectionRadius && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = point.transform;
            }
        }

        // Si on a trouvé un point, le définir comme cible
        if (nearestPoint != null)
        {
            target = nearestPoint;
            isWaitingForInput = false;
        }
    }

    void MoveTowardsTarget()
    {
        if (target == null) return;

        // Se déplacer vers la cible
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Si on est très proche de la cible, on la désactive
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            transform.position = target.position;
            target.gameObject.SetActive(false);
            target = null;  // Réinitialiser la cible pour chercher un autre point
        }
    }

    void HandleUserInput()
    {
        if (potentialTargets.Count == 0) return;

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
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
