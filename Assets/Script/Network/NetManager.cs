using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;

public class NetManager : MonoBehaviour
{
    static Socket socket;
    static byte[] readBuff = new byte[1024];
    public delegate void MsgListener(String str);
    private static Dictionary<string, MsgListener> Listeners = new Dictionary<string, MsgListener>();
    static List<String> msgList = new List<string>();

    public static void AddListener(string msgName, MsgListener listener)
    {
        Listeners[msgName] = listener;
    }

    public static string GetDesc()
    {
        if (socket == null || !socket.Connected) return " ";
        return socket.LocalEndPoint.ToString();
    }

    public static void Connect(string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ip, port);   // remember to develop this to be async later
        socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            msgList.Add(recvStr);
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Receive failed " + ex.ToString());
        }
    }

    public static void Send(string sendStr)
    {
        if (socket == null || !socket.Connected) return;
        byte[] sendByte = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendByte);  // try to make this asnyc if possible, not must
    }

    public static void Update()
    {
        if (msgList.Count <= 0) return;
        String msgStr = msgList[0];
        msgList.RemoveAt(0);
        string[] split = msgStr.Split('|');
        string msgName = split[0];
        string msgArgs = split[1];
        if (Listeners.ContainsKey(msgName)) Listeners[msgName](msgArgs);
    }

}
