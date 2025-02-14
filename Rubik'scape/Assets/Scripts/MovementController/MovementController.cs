using UnityEngine;
using System.Collections.Generic;

public class MoveToEmptyObject : MonoBehaviour
{
    public float detectionRadius = 5f;  // Rayon de détection
    public float speed = 4f;  // Vitesse de déplacement
    private Transform target;  // Cible à atteindre
    private List<Transform> potentialTargets = new List<Transform>();  // Liste des cibles potentielles
    private bool isWaitingForInput = false;  // Si on attend l'entrée de l'utilisateur

    void Update()
    {
        // Si on n'a pas de cible, chercher les empty objects à la même distance
        if (target == null)
        {
            FindNearestEmptyObjects();
        }

        // Si on attend l'entrée de l'utilisateur, vérifier si une touche est pressée
        if (isWaitingForInput)
        {
        // Ajouter un Debug.Log ici pour afficher le nombre de cibles disponibles
        Debug.Log("Nombre de cibles à proximité: " + potentialTargets.Count);

            HandleUserInput();
        }

        // Si une cible est définie, se déplacer vers elle
        if (target != null && !isWaitingForInput)
        {
            MoveTowardsTarget();
        }
    }

    void FindNearestEmptyObjects()
    {
        // Chercher tous les GameObjects dans la scène ayant le tag "EmptyObject"
        GameObject[] emptyObjects = GameObject.FindGameObjectsWithTag("EmptyObject");

        potentialTargets.Clear();  // Réinitialiser la liste des cibles potentielles
        float minDistance = Mathf.Infinity;

        foreach (GameObject emptyObject in emptyObjects)
        {
            // Vérifier la distance entre ce GameObject et l'objet actuel
            float distance = Vector3.Distance(transform.position, emptyObject.transform.position);

            if (distance < minDistance && distance <= detectionRadius)
            {
                minDistance = distance;  // Trouver la distance minimale
                potentialTargets.Clear();  // Réinitialiser la liste si une nouvelle distance minimale est trouvée
                potentialTargets.Add(emptyObject.transform);
            }
            else if (Mathf.Abs(distance - minDistance) < 0.01f) // Si la distance est égale à la distance minimale
            {
                potentialTargets.Add(emptyObject.transform);  // Ajouter à la liste des cibles potentielles
            }
        }

        // Si plusieurs cibles sont trouvées, attendre l'entrée de l'utilisateur
        if (potentialTargets.Count > 1)
        {
            isWaitingForInput = true;
            target = null;  // Réinitialiser la cible pour attendre l'input
        }
        else if (potentialTargets.Count == 1)
        {
            target = potentialTargets[0];  // Si une seule cible, se déplacer directement
        }
    }

    void HandleUserInput()
    {
        if (potentialTargets.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            target = potentialTargets[0];  // Choisir la première cible
            isWaitingForInput = false;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            target = potentialTargets[1];  // Choisir la deuxième cible (si elle existe)
            isWaitingForInput = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && potentialTargets.Count > 2)
        {
            target = potentialTargets[2];  // Choisir la troisième cible (si elle existe)
            isWaitingForInput = false;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && potentialTargets.Count > 3)
        {
            target = potentialTargets[3];  // Choisir la quatrième cible (si elle existe)
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
            target.gameObject.SetActive(false);  // Désactiver l'objet Empty
            target = null;  // Réinitialiser la cible pour chercher un autre Empty Object
        }
    }

    // Pour déboguer visuellement le rayon de détection dans l'éditeur
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);  // Afficher un cercle de détection
    }
}
