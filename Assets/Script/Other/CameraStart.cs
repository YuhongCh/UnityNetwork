using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStart : MonoBehaviour
{
    [SerializeField] private GameObject Player;

    public void setPlayer(GameObject player)
    {
        Player = player;
        Vector3 pos = Player.transform.position;
        pos.y += 5;
        transform.position = pos;
    }
}
