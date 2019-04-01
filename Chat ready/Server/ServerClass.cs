using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using ChatMessage;

namespace Server
{
    class ServerClass
    {
        List<ChatClient> clients;
        IPAddress ipServer;
        IPEndPoint endPoint;
        Socket sListener;
        public ServerClass(string ipAdress, int port)
        {
            clients = new List<ChatClient>();
            ipServer = IPAddress.Parse(ipAdress);
            endPoint = new IPEndPoint(ipServer, port);
            sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Listen(int count)
        {
            try
            {
                sListener.Bind(endPoint);
                sListener.Listen(count);
                while (true)
                {
                    Socket client = sListener.Accept();
                    var tmp = new ChatClient(client, this);
                    clients.Add(tmp);
                    tmp.ReciveMessageAsync();

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }
        public void BroadcastMessage(byte[] msg, Guid id)
        {
            foreach (var client in clients)
            {
                if (client.info.id != id)
                {
                    client.SendMessage(msg);
                }
            }
        }
        public void SendPrivateMessage(byte[] msg, UserInfo reciever)
        {
            var r = clients.FirstOrDefault(c => c.info.CompareTo(reciever) == 0);
            r.SendMessage(msg);
        }
        public void SendUsersTo(Guid id,Socket c)
        {
            foreach (var client in clients)
            {
                if (client.info.id != id)
                {
                    ChatMessage.Message tmp = new ChatMessage.Message(client.info, ChatMessage.MessageType.UserData, "", DateTime.Now);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, tmp);
                        c.Send(stream.ToArray());
                        Console.WriteLine("Send {0} To{1}", tmp.username.username, id);
                        Thread.Sleep(50);
                    }
                }
            }
        }
        public void RemoveClient(Guid id)
        {
            ChatClient del = clients.Find(c=>c.info.id==id);
            clients.Remove(del);
        }
       

        
    }
}
