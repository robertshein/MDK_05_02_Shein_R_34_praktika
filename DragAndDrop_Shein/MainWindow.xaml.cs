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
using System.Windows.Threading;

namespace DragAndDrop_Shein
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 1000 / 60);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e) 
        {
            image.Margin = new Thickness(Mouse.GetPosition(this).X - 25, Mouse.GetPosition(this).Y - 25, 0, 0);
        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
