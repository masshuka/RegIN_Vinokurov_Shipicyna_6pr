using RegIN_Vinokurov_Shipicyna_6pr.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegIN_Vinokurov_Shipicyna_6pr
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;
        public User UserLogIn = new User();

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            OpenPage(new Pages.Login());
        }

        public void OpenPage(Page page)
        {
            DoubleAnimation StartAnimation = new DoubleAnimation();
            StartAnimation.From = 1;
            StartAnimation.To = 0;
            StartAnimation.Duration = TimeSpan.FromSeconds(0.6);

            StartAnimation.Completed += delegate
            {
                frame.Navigate(page);

                DoubleAnimation EndAnimation = new DoubleAnimation();
                EndAnimation.From = 0;
                EndAnimation.To = 1;
                EndAnimation.Duration = TimeSpan.FromSeconds(1.2);

                frame.BeginAnimation(Frame.OpacityProperty, EndAnimation);
            };

            frame.BeginAnimation(Frame.OpacityProperty, StartAnimation);
        }
    }
}
