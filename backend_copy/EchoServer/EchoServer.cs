using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    public class ClientState
    {
        public Socket socket;
        public byte[] readBuff = new byte[1024];

        public int hp = -100;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float eulY = 0;
    }

    class MainClass
    {
        static Socket listenfd;
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState> ();

        public static void Main (string[] args)
        {
            Console.WriteLine("Hello World");
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            listenfd.Listen(0);
            Console.WriteLine("[Server] start successfully");
            listenfd.BeginAccept(AcceptCallback, listenfd);
            Console.ReadLine();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("[Server] Accept");
                Socket listenfd = (Socket)ar.AsyncState;
                Socket clientfd = listenfd.EndAccept(ar);

                ClientState state = new ClientState();
                state.socket = clientfd;
                clients.Add(clientfd, state);

                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
                listenfd.BeginAccept(AcceptCallback, listenfd);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Socket Accept failed " + ex.ToString());
            }
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            ClientState state = (ClientState)ar.AsyncState;
            Socket clientfd = state.socket;
            int count = clientfd.EndReceive(ar);
            try
            {
                // special condition to close clientfd
                if (count <= 0)
                {
                    MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                    object[] ob = {state};
                    mei.Invoke(null, ob);

                    clientfd.Close();
                    clients.Remove(clientfd);
                    Console.WriteLine("Socket close");
                    return;
                }

                // receive message
                string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
                Console.WriteLine("[Server] received " + recvStr);

                // process the received message
                string[] splits = recvStr.Split('|');
                string msgName = splits[0];
                string msgArgs = splits[1];
                MethodInfo mi = typeof(MsgHandler).GetMethod("Msg" + msgName);
                object[] o = {state, msgArgs};
                mi.Invoke(null, o);
                
                /* send back message
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes(DateTime.Now.ToString() + " " + recvStr);
                foreach (ClientState s in clients.Values)
                {
                    s.socket.Send(sendBytes);   // used send because too lazy to write async
                }*/
                // Console.WriteLine("[Server] respond " + "echo " + recvStr);
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
            }
            catch(SocketException ex)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] ob = {state};
                mei.Invoke(null, ob);

                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Socket Receive failed " + ex.ToString() + " and close Socket");
            }
        }

        public static void Send(ClientState cs, string sendStr)
        {
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            cs.socket.Send(sendBytes);
        }
    }
}
