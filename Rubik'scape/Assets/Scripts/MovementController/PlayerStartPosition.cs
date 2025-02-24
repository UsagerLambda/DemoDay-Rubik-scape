using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartPosition : MonoBehaviour
{
    public GameObject player;
    public GameObject self;
    void OnEnable() {
        Debug.Log("PlayerStartPosition Appelé !");
        GameObject StartTile = GameObject.FindWithTag("start");
        if (StartTile != null) {
            Debug.Log("success: Start was found");
            player.SetActive(true);
            player.transform.position = StartTile.transform.position;
            player.transform.rotation = StartTile.transform.rotation * Quaternion.Euler(0, 180, 0);
            player.transform.parent = StartTile.transform;
            self.SetActive(false);

        } else {
            Debug.Log("error: Start not found");
        }
    }
}
