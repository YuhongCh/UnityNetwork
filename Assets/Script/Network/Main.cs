using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject humanPrefab;
    public BaseHuman myHuman;
    public Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>();

    void Start()
    {
        NetManager.AddListener("Enter", OnEnter);
        NetManager.AddListener("Move", OnMove);
        NetManager.AddListener("Leave", OnLeave);
        NetManager.AddListener("List", OnList);
        NetManager.AddListener("Attack", OnAttack);
        NetManager.AddListener("Die", OnDie);
        NetManager.Connect("127.0.0.1", 8888);

        // create instance
        GameObject obj = (GameObject)Instantiate(humanPrefab);
        obj.transform.position = new Vector3(Random.Range(0f,10f), 3.5f, Random.Range(0f, 10f));
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.desc = NetManager.GetDesc();

        // send message to server
        Vector3 pos = myHuman.transform.position;
        Vector3 rot = myHuman.transform.eulerAngles;
        string sendStr = "Enter|" + NetManager.GetDesc() + ",";
        sendStr += pos.x + "," + pos.y + "," + pos.z + "," + rot.y;
        NetManager.Send(sendStr);

        // ask the player list from server
        NetManager.Send("List|");

    }

    void Update()
    {
        NetManager.Update();
    }

    void OnList(string msg)
    {
        Debug.Log("OnList " + msg);
        string[] split = msg.Split(',');
        int count = (split.Length-1) / 6;
        for (int i = 0; i < count; i++)
        {
            string desc = split[i*6];
            float x = float.Parse(split[i*6 + 1]);
            float y = float.Parse(split[i*6 + 2]);
            float z = float.Parse(split[i*6 + 3]);
            float eulY = float.Parse(split[i*6 + 4]);
            float hp = float.Parse(split[i*6 + 5]);

            if (desc == NetManager.GetDesc()) continue;
            Debug.Log(desc);
            GameObject obj = (GameObject)Instantiate(humanPrefab);
            obj.transform.position = new Vector3(x, y, z);
            obj.transform.eulerAngles = new Vector3(0, eulY, 0);
            BaseHuman h = obj.AddComponent<SyncHuman>();
            h.desc = desc;
            otherHumans.Add(desc, h);
        }
    }

    void OnEnter(string msg)
    {
        Debug.Log("Enter " + msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        if (desc == NetManager.GetDesc()) return;
        // load the x y z position and eulY
        GameObject obj = (GameObject)Instantiate(humanPrefab);
        obj.transform.position = new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3]));
        obj.transform.eulerAngles = new Vector3(0, float.Parse(split[4]), 0);
        BaseHuman h = obj.AddComponent<SyncHuman>();
        h.desc = desc;
        otherHumans.Add(desc, h);
    }

    void OnMove(string msg)
    {
        Debug.Log("OnMove " + msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        if (!otherHumans.ContainsKey(desc)) return;
        BaseHuman h = otherHumans[desc];
        Vector3 targetPos = new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3]));
        h.MoveTo(targetPos);
    }

    void OnLeave(string msg)
    {
        Debug.Log("Leave " + msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        if (!otherHumans.ContainsKey(desc)) return;
        BaseHuman h = otherHumans[desc];
        Destroy(h.gameObject);
        otherHumans.Remove(desc);
    }

    void OnAttack(string msg)
    {
        Debug.Log("OnAttack" + msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        float eulY = float.Parse(split[1]);
        if (!otherHumans.ContainsKey(desc)) return;
        SyncHuman h = (SyncHuman) otherHumans[desc];
        h.SyncAttack(eulY);
    }

    void OnDie(string msg)
    {
        Debug.Log("OnDie " + msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        if (desc == myHuman.desc)
        {
            Debug.Log("Game Over!!!");
            return;
        }
        if (!otherHumans.ContainsKey(desc)) return;
        SyncHuman h = (SyncHuman) otherHumans[desc];
        h.gameObject.SetActive(false);
    }
}