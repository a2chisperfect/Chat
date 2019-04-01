using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Chat
{
    class ClosableTab :TabItem
    {
        public string Title
        {
            set
            {
                ((CloseableHeader)this.Header).label_TabTitle.Content = value;
            }
            get
            {
                return ((CloseableHeader)this.Header).label_TabTitle.Content.ToString();
            }
        }

        public bool CloseIsEnabled
        {
            set
            {
                ((CloseableHeader)this.Header).button_close.IsEnabled = value;
            }
        }

        public ClosableTab()
        {
            CloseableHeader closableTabHeader = new CloseableHeader();
            this.Header = closableTabHeader;
            closableTabHeader.button_close.Click += new RoutedEventHandler(button_close_Click);
        }
        //protected override void OnMouseEnter(MouseEventArgs e)
        //{
        //    base.OnMouseEnter(e);
        //    ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Visible;
        //}

        // Override OnMouseLeave - Hide the Close Button (If it is NOT selected)
        //protected override void OnMouseLeave(MouseEventArgs e)
        //{
        //    base.OnMouseLeave(e);
        //    if (!this.IsSelected)
        //    {
        //        ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
        //    }
        //}

        void button_close_Click(object sender, RoutedEventArgs e)
        {
            if (this.Title != "Lobby")
            {
                ((TabControl)this.Parent).Items.Remove(this);
            }

        }

    }
}
