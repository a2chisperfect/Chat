using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ChatMessage;

namespace Chat
{
    class ChatClient :IDisposable
    {
        private Socket client;
        private IPAddress ipServer;
        private IPEndPoint endPoint;
        private UserInfo userName;
        private Dispatcher dispatcher;
        private MainWindow window;

        public EventHandler<PrivateMessageEventArgs>PrivateMessage;

        public ObservableCollection<Users>users{get;set;}
        public ObservableCollection<Message> history { get; set; }

        public ChatClient(string name,string ipAdress, int port, MainWindow w)
        {
            ipServer = IPAddress.Parse(ipAdress);
            endPoint = new IPEndPoint(ipServer, port);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            history = new ObservableCollection<Message>();
            users = new ObservableCollection<Users>();
            dispatcher = Dispatcher.CurrentDispatcher;
            userName = new UserInfo(name);
            window = w;
        }

        public void Connect()
        {
            try
            {
                client.Connect(endPoint);
                SendMyInfo();
            }
            catch
            {
                throw;
            }
           
        }
        public void SendMyInfo()
        {
            try
            {

                Message tmp = new Message(userName, MessageType.UserData, string.Empty, DateTime.Now);
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tmp);
                    client.Send(stream.ToArray());
                    //tmp.myDisplayUsername = "I :";
                }
            }
            catch
            {
                throw;
            }
        }

        public void SendQuitMessage()
        {
            try
            {
                Message tmp = new Message(userName, MessageType.Quit, string.Empty, DateTime.Now);
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tmp);
                    client.Send(stream.ToArray());
                    //tmp.myDisplayUsername = "I :";
                }
            }
            catch
            {
                throw;
            }
        }
        public void SendMessageTo(UserInfo receiver, string message)
        {
            try
            {
                //Message tmp = new Message("",": " + userName,message,DateTime.Now);
                Message tmp = new Message((UserInfo)userName.Clone(),receiver, MessageType.Private, message, DateTime.Now);
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tmp);
                    client.Send(stream.ToArray());
                    //tmp.myDisplayUsername = "I :";
                    tmp.username.username = "I";
                    var t = users.FirstOrDefault(c => c.user.CompareTo(tmp.receiver) == 0);
                    t.history.Add(tmp);
                    //history.Add(tmp);
                }
            }
            catch
            {
                throw;
            }
        }
        public void SendMessage(string message)
        {
            try
            {
                //Message tmp = new Message("",": " + userName,message,DateTime.Now);
                Message tmp = new Message((UserInfo)userName.Clone(), MessageType.ToAll, message, DateTime.Now);
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tmp);
                    client.Send(stream.ToArray());
                    //tmp.myDisplayUsername = "I :";
                    tmp.username.username = "I";
                    history.Add(tmp);
                }
            }
            catch
            {
                throw;
            }
        }
        public Task ReciveMessageAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    byte[] bytes = new byte[1024];
                    while (true)
                    {
                        using(var stream = new MemoryStream())
                        {
                            do
                            {
                                int byteRec = client.Receive(bytes);
                                stream.Write(bytes, 0, byteRec);

                            } while (client.Available > 0);
                            if (stream.Length !=0)
                            {
                                stream.Position = 0;
                                BinaryFormatter formatter = new BinaryFormatter();
                                Message tmp = (Message)formatter.Deserialize(stream);
                                if(tmp.Type == MessageType.UserData)
                                {
                                    dispatcher.BeginInvoke((ThreadStart)delegate() { users.Add(new Users(tmp.username)); });
                                }
                                if (tmp.Type == MessageType.Private)
                                {
                                    dispatcher.BeginInvoke((ThreadStart)delegate() { var t = users.FirstOrDefault(u => u.user.CompareTo(tmp.username) == 0);
                                                                                     if(t!=null)
                                                                                     {
                                                                                         t.history.Add(tmp);
                                                                                         PrivateMessage(this,new PrivateMessageEventArgs(t));
                                                                                      }
                                                                                    });
                                }
                                if (tmp.Type == MessageType.Quit)
                                {
                                    dispatcher.BeginInvoke((ThreadStart)delegate()
                                    {
                                        var t = users.FirstOrDefault(u => u.user.CompareTo(tmp.username) == 0);
                                        if (t != null)
                                        {
                                            users.Remove(t);
                                            //PrivateMessage(this, new PrivateMessageEventArgs(t));
                                        }
                                    });
                                }
                                if (tmp.Type == MessageType.ToAll)
                                {
                                    dispatcher.BeginInvoke((ThreadStart)delegate() { history.Add(tmp); });
                                }
                               
                            }
                        }

                    }
                }
                catch(Exception ex)
                {
                    window.Dispatcher.BeginInvoke((ThreadStart)delegate() { MessageBox.Show(ex.Message); window.Close(); });
                }
                finally
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            });
        }

        public void Dispose()
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }

    class PrivateMessageEventArgs:EventArgs
    {
        public Users user { get; set; }
        public PrivateMessageEventArgs(Users user)
        {
            this.user = user;
        }
    }

    class Users
    {
        public UserInfo user { get; set; }
        public ObservableCollection<Message> history { get; set; }
        public Users(UserInfo user)
        {
            this.user = user;
            history = new ObservableCollection<Message>();
        }
    }
}
