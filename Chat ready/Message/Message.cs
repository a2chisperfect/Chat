using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ChatMessage
{
    [Serializable]
    public enum MessageType { UserData, ToAll, Private, Quit };

    [Serializable]
    public class Message
    {
        public MessageType Type { get; set; }
        //public string myDisplayUsername { get; set; }
        public UserInfo username { get; set; }
        public UserInfo receiver { get; set; }
        public string message { get; set; }
        public DateTime time { get; set; }

        public Message(UserInfo user, MessageType type, string message, DateTime time)
        {
            this.Type = type;
            username = user;
            //this.myDisplayUsername = myDisplayUsername;
            //this.username = username;
            this.message = message;
            this.time = time;
        }
        public Message(UserInfo user,UserInfo receiver, MessageType type, string message, DateTime time)
        {
            this.Type = type;
            this.receiver = receiver;
            username = user;
            //this.myDisplayUsername = myDisplayUsername;
            //this.username = username;
            this.message = message;
            this.time = time;
        }
    }

    [Serializable]
    public class UserInfo :ICloneable,IComparable
    {
        public Guid id { get; set; }
        public string username { get; set; }
        public UserInfo() { }

        public UserInfo(string name)
        {
            username = name;
            id = Guid.NewGuid();
        }

        public object Clone()
        {
            return (object)new UserInfo() { id = this.id, username = this.username };
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            UserInfo otherUser= obj as UserInfo;
            if (otherUser != null)
                return this.id.CompareTo(otherUser.id);
            else
                throw new ArgumentException("Object is not a UserInfo");
        }
    }

}
