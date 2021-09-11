using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Linq;
using UnityEngine.UI;
using System;

public class NetManager : MonoBehaviour
{
    static Socket socket;
    static byte[] readBuff = new byte[1024];
    private static int buffCounter = 0;
    private static string recvStr = "";
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
        socket.BeginReceive(readBuff, buffCounter, 1024 - buffCounter, 0, ReceiveCallback, socket);
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            buffCounter += count;
            // recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            OnReceiveData();

            msgList.Add(recvStr);
            socket.BeginReceive(readBuff, buffCounter, 1024 - buffCounter, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Receive failed " + ex.ToString());
        }
    }

    private static void OnReceiveData()
    {
        Debug.Log("[Recv 1] buffCounter = " + buffCounter);
        Debug.Log("[Recv 2] readBuff = " + BitConverter.ToString(readBuff));
        if (buffCounter <= 2) return;
        Int16 bodyLength = BitConverter.ToInt16(readBuff, 0);
        Debug.Log("[Recv 3] bodyLength = " + bodyLength);
        if (buffCounter < 2 + bodyLength) return;
        string s = System.Text.Encoding.UTF8.GetString(readBuff, 2, bodyLength);
        Debug.Log("[Recv 4] s = " + s);

        int start = 2 + bodyLength;
        int count = buffCounter - start;
        Array.Copy(readBuff, start, readBuff, 0 ,count);
        buffCounter -= start;
        Debug.Log("[Recv 5] buggCounter = " + buffCounter);
        recvStr = s + "\n" + recvStr;
        OnReceiveData();
    }

    public static void Send(string sendStr)
    {   
        if (socket == null || !socket.Connected) return;
        byte[] bodyBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        Int16 length = (Int16)bodyBytes.Length;
        byte[] lengthBytes = BitConverter.GetBytes(length);

        if (!BitConverter.IsLittleEndian)
        {
            Debug.Log("[Send] Reverse lenBytes");
            lengthBytes.Reverse();
        }

        byte[] sendBytes = lengthBytes.Concat(bodyBytes).ToArray();
        socket.Send(sendBytes);  // try to make this asnyc if possible, not must

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
