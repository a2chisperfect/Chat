using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Chat
{
    class Commands
    {
        private static RoutedUICommand sendMessageCommand;
        static Commands()
        {
            sendMessageCommand = new RoutedUICommand("SendMessageCommand", "SendMessageCommand", typeof(Commands));
        }
        public static RoutedUICommand SendMessageCommand
        {
            get
            {
                return sendMessageCommand;
            }
        }
    }
}
