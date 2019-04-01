using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using ChatMessage;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static ChatClient client;
        public MainWindow()
        {
            InitializeComponent();
            AddDoubleClickEventStyle(lbUsers, dGSerials_MouseDoubleClick);
            ResourceDictionary myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("Styles.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);

            try
            {
                LogIn dialog = new LogIn();
                if (dialog.ShowDialog() == true)
                {
                client = new ChatClient(dialog.username.Text,"127.0.0.1", 11000,this);
                client.Connect();

                AddLobbyTab();
                lbUsers.ItemsSource = client.users;
                client.PrivateMessage += PrivateMessageHandler;
                client.ReciveMessageAsync();
                }
                else
                {
                    this.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }

            CommandBinding binding = new CommandBinding(Commands.SendMessageCommand);
            binding.CanExecute += binding_CanExecute;
            binding.Executed += binding_Executed;
            this.CommandBindings.Add(binding);
            btnSend.Command = Commands.SendMessageCommand;
            
        }

        void binding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(tbMessage.Text))
                {
                    if ((Tabs.SelectedItem as ClosableTab).Tag is UserInfo)
                    {
                        client.SendMessageTo((Tabs.SelectedItem as ClosableTab).Tag as UserInfo, tbMessage.Text);
                    }
                    else if ((Tabs.SelectedItem as ClosableTab).Title.ToString() == "Lobby")
                    {
                        client.SendMessage(tbMessage.Text);
                    }
                }
                tbMessage.Text = "";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void binding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if ((Tabs.SelectedItem as ClosableTab).Title != "Lobby" && client.users.FirstOrDefault(c => c.user.CompareTo((Tabs.SelectedItem as ClosableTab).Tag as UserInfo) == 0) == null)
            {

                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }
        private void PrivateMessageHandler(object sender, PrivateMessageEventArgs e)
        {
            foreach (var item in Tabs.Items)
            {
                if (((item as ClosableTab).Tag as UserInfo) == e.user.user)
                    return;
            }
            AddNewTab(e.user,false);
        }

        private void AddDoubleClickEventStyle(ListBox listBox, MouseButtonEventHandler mouseButtonEventHandler)
        {
            if (listBox.ItemContainerStyle == null)
                listBox.ItemContainerStyle = new Style(typeof(ListBoxItem));
            listBox.ItemContainerStyle.Setters.Add(new EventSetter()
            {
                Event = MouseDoubleClickEvent,
                Handler = mouseButtonEventHandler
            });
        }

        public void dGSerials_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                var user = ((sender as ListBoxItem).Content as Users).user;
                foreach (var item in Tabs.Items)
                {
                    if (((item as ClosableTab).Tag as UserInfo) == user)
                        return;
                }
                AddNewTab(client.users.FirstOrDefault(u=>u.user.CompareTo(user)==0),true);

            }
        }
        private void AddLobbyTab()
        {
            ClosableTab t = new ClosableTab();
            t.Title = "Lobby";
            t.Tag = null;
            t.CloseIsEnabled = false;
            ListBox tmp = new ListBox();
            tmp.ItemTemplate = (DataTemplate)Application.Current.FindResource("ListBoxItemTemplate");
            tmp.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            t.Content = tmp;
            Tabs.Items.Add(t);
            Tabs.SelectedItem = t;
            tmp.ItemsSource = client.history;
        }

        private void AddNewTab(Users user, bool isSelected)
        {
            ClosableTab t = new ClosableTab();
            t.Title = user.user.username;
            t.Tag = user.user;
            ListBox tmp = new ListBox();
            tmp.ItemTemplate = (DataTemplate)Application.Current.FindResource("ListBoxItemTemplate");
            tmp.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            t.Content = tmp;
            Tabs.Items.Add(t);
            tmp.ItemsSource = user.history;
            if(isSelected)
            {
                Tabs.SelectedItem = t;
            }
            
            
        }
       

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (client != null)
                {
                    client.SendQuitMessage();
                }
            }
            catch { }
        }
           
    }
}
