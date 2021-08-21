using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    private Vector3 lastPos;

    private void Update()
    {
        if (lastPos == null) return;
        transform.position += (Player.transform.position - lastPos);
        lastPos = Player.transform.position;
    }

    public void setPlayer(GameObject player)
    {
        Player = player;
        lastPos = Player.transform.position;
    }
}
