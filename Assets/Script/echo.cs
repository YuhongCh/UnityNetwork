using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;

public class echo : MonoBehaviour
{
    Socket socket;
    public InputField InputField;
    public Text text;

    byte[] readBuff = new byte[1024];
    string recvStr = "";

    public void Connection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect("127.0.0.1", 8888, ConnectCallBack, socket);
    }

    public void ConnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Success");
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Connect failed " + ex.ToString());
        }
    }

    public void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            string s = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            recvStr = s + "\n" + recvStr;
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Receive failed " + ex.ToString());
        }
    }

    public void Send()
    {
        string sendStr = InputField.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        InputField.text = ""; // clear this out
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    public void SendCallback(IAsyncResult ar)
    {
        try{
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndSend(ar);
            Debug.Log("Socket Send success " + count);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Send failed " + ex.ToString());
        }
        
    }

    public void Update()
    {
        text.text = recvStr;
    }
}
