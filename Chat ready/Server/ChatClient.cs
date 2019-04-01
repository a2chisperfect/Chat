using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ChatMessage;

namespace Server
{
    class ChatClient : IDisposable
    {
        public UserInfo info { get; set; }
        private Socket client;
        private ServerClass server;
        
        public ChatClient(Socket socket, ServerClass server)
        {
            //id = Guid.NewGuid();
            client = socket;
            this.server = server;
            info = null;
        }

        //public void SendMessage(byte [] msg)
        //{
        //    client.Send(msg);
        //}
        public Task SendMessage(byte[] msg)
        {
            return Task.Run(() =>
            {
                try
                {
                    client.Send(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public Task GetOtherUsers()
        {
            return Task.Run(() =>
                {
                    try
                    {
                        server.SendUsersTo(info.id, client);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
        }

        public Task ReciveMessageAsync()
        {

            return Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        byte[] buf = new byte[1024];
                        using (var stream = new MemoryStream())
                        {
                            do
                            {
                                int byteRec = client.Receive(buf);
                                stream.Write(buf, 0, byteRec);

                            } while (client.Available > 0);
                            if (stream.Length != 0)
                            {
                                stream.Position = 0;

                                BinaryFormatter formatter = new BinaryFormatter();
                                Message tmp = (Message)formatter.Deserialize(stream);
                                stream.Position = 0;
                                if (tmp.Type == MessageType.UserData)
                                {
                                    info = tmp.username;
                                    Console.WriteLine(tmp.username.username);
                                    GetOtherUsers();
                                }
                                if(tmp.Type == MessageType.Private)
                                {
                                    server.SendPrivateMessage(stream.ToArray(), tmp.receiver);
                                }
                                else
                                {
                                    server.BroadcastMessage(stream.ToArray(), info.id);
                                }
                                
                            }
                           
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                finally
                {
                    server.RemoveClient(info.id);
                }

            });
        }
        
        public void Dispose()
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
