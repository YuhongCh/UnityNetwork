using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    [SerializeField] private Camera playerCamera;
    private GameObject CameraStuff;
    new void Start()
    {
        base.Start();
        try
        {
            CameraStuff = GameObject.FindGameObjectsWithTag("Cameras")[0];
        }
        catch
        {
            Debug.Log("Failed to get the player camera");
        }
        playerCamera = CameraStuff.transform.GetChild(0).gameObject.GetComponent<Camera>();
        CameraStuff.GetComponent<CameraCtrl>().setPlayer(gameObject);
        playerCamera.GetComponent<CameraStart>().setPlayer(gameObject);
    }

    new void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.collider.tag == "Terrain")
            {
                MoveTo(hit.point);

                // send to server
                string sendStr = "Move|";
                sendStr += NetManager.GetDesc() + ",";
                sendStr += hit.point.x + "," + hit.point.y + "," + hit.point.z + ",";
                NetManager.Send(sendStr);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (isAttacking) return;
            if (isMoving) return;
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            transform.LookAt(hit.point);
            Attack();

            // send attack info to server
            string sendStr = "Attack|";
            sendStr += NetManager.GetDesc() + ",";
            sendStr += transform.eulerAngles.y + ",";
            NetManager.Send(sendStr);

            // check if hit target, if so tell server
            HitTarget();
        }
    }

    private void HitTarget()
    {   
        RaycastHit hit;
        Vector3 lineEnd = transform.position + 0.5f * Vector3.up;
        Vector3 lineStart = lineEnd + 20 * transform.forward;
        if (Physics.Linecast(lineStart, lineEnd, out hit))
        {
            GameObject obj = hit.collider.gameObject;
            if (obj == gameObject) return ;
            SyncHuman h = obj.GetComponent<SyncHuman>();
            if (h == null) return;
            string sendStr = "Hit|" + NetManager.GetDesc() + "," + h.desc + ",";
            NetManager.Send(sendStr);

        }
    }
}
