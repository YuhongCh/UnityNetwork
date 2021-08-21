using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;


public class Poll: MonoBehaviour{
    Socket socket;
    public InputField InputField;
    public Text text;
    public void Connection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 8888);
    }

    public void Send()
    {
        
    }

    public void Update()
    {
        if (socket == null)
        {
            return;
        }
        if (socket.Poll(0, SelectMode.SelectRead))
        {
            byte[] readBuff = new byte[1024];
            int count = socket.Receive(readBuff);
            string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            text.text = recvStr;
        }
    }
}