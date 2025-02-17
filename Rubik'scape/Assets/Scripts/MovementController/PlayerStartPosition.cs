using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartPosition : MonoBehaviour
{
    public GameObject player;
    void Start()
    {
        GameObject StartTile = GameObject.FindWithTag("start");
        if (StartTile != null) {
            Debug.Log("success: Start was found");
            player.SetActive(true);
            player.transform.position = StartTile.transform.position;
            player.transform.parent = StartTile.transform;

        } else {
            Debug.Log("error: Start not found");
        }
    }
}
