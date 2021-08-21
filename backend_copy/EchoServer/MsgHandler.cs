using System;
using System.Collections.Generic;


namespace EchoServer
{
    class MsgHandler
    {
        public static void MsgEnter(ClientState c, string msgArgs)
        {
            string[] split = msgArgs.Split(',');
            string desc = split[0];
            c.hp = 100;
            c.x = float.Parse(split[1]);
            c.y = float.Parse(split[2]);
            c.z = float.Parse(split[3]);
            c.eulY = float.Parse(split[4]);

            string sendStr = "Enter|" + msgArgs;
            foreach (ClientState cs in MainClass.clients.Values)
            {
                MainClass.Send(cs, sendStr);
            }
        }

        public static void MsgList(ClientState c, string msgArgs)
        {
            string sendStr = "List|";
            foreach (ClientState cs in MainClass.clients.Values)
            {
                sendStr += cs.socket.RemoteEndPoint.ToString() + ",";
                sendStr += cs.x.ToString() + "," + cs.y.ToString() + "," + cs.z.ToString() + ",";
                sendStr += cs.eulY.ToString() + "," + cs.hp.ToString() + ",";
            }
            MainClass.Send(c, sendStr);
        }

        public static void MsgMove(ClientState c, string msgArgs)
        {
            string[] split = msgArgs.Split(',');
            string desc = split[0];
            c.x = float.Parse(split[1]);
            c.y = float.Parse(split[2]);
            c.z = float.Parse(split[3]);
            string sendStr = "Move|" + msgArgs;
            foreach (ClientState cs in MainClass.clients.Values)
            {
                MainClass.Send(cs, sendStr);
            }
        }

        public static void MsgAttack(ClientState c, string msgArgs)
        {
            string sendStr = "Attack|" + msgArgs;
            foreach(ClientState cs in MainClass.clients.Values)
            {
                MainClass.Send(cs, sendStr);
            }
        }

        public static void MsgHit(ClientState c, string msgArgs)
        {
            string[] split = msgArgs.Split(',');
            string attDesc = split[0];
            string hitDesc = split[1];
            ClientState hitCS = null;
            foreach(ClientState cs in MainClass.clients.Values)
            {
                if (cs.socket.RemoteEndPoint.ToString() == hitDesc) hitCS = cs;
            }
            if (hitCS == null) return;
            hitCS.hp -= 25;
            if (hitCS.hp <= 0)
            {
                string sendStr = "Die|" + hitCS.socket.RemoteEndPoint.ToString();
                foreach (ClientState cs in MainClass.clients.Values)
                {
                    MainClass.Send(cs, sendStr);
                }
            }
        }
        
    }

}
