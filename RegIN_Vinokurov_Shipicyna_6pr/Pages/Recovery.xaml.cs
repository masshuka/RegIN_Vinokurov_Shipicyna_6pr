using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using RegIN_Vinokurov_Shipicyna_6pr.Classes;

namespace RegIN_Vinokurov_Shipicyna_6pr.Pages
{
    /// <summary>
    /// Логика взаимодействия для Recovery.xaml
    /// </summary>
    public partial class Recovery : Page
    {
        string OldLogin;
        bool IsCapture = false;
        public Recovery()
        {
            InitializeComponent();
            MainWindow.mainWindow.UserLogIn.HandlerCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.HandlerInCorrectLogin += InCorrectLogin;
            Capture.HandlerCorrectCapture += CorrectCapture;
        }

        public void SetNotification(string Message, SolidColorBrush _Color)
        {
            LNameUser.Content = Message;
            LNameUser.Foreground = _Color;
        }

        /// <summary>
        /// Метод правильного ввода логина
        /// </summary>
        private void CorrectLogin()
        {
            if (OldLogin != TbLogin.Text)
            {
                SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);

                try
                {
                    BitmapImage blImg = new BitmapImage();
                    MemoryStream ms = new MemoryStream(MainWindow.mainWindow.UserLogIn.Image);

                    blImg.BeginInit();
                    blImg.StreamSource = ms;
                    blImg.EndInit();

                    ImageSource imgSrc = blImg;

                    DoubleAnimation StartAnimation = new DoubleAnimation();
                    StartAnimation.From = 1;
                    StartAnimation.To = 0;
                    StartAnimation.Duration = TimeSpan.FromSeconds(0.6);

                    StartAnimation.Completed += delegate
                    {
                        IUser.Source = imgSrc;

                        DoubleAnimation EndAnimation = new DoubleAnimation();
                        EndAnimation.From = 0;
                        EndAnimation.To = 1;
                        EndAnimation.Duration = TimeSpan.FromSeconds(1.2);

                        IUser.BeginAnimation(Image.OpacityProperty, EndAnimation);
                    };

                    IUser.BeginAnimation(Image.OpacityProperty, StartAnimation);
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp.Message);
                }

                OldLogin = TbLogin.Text;
                SendNewPassword();
            }
        }

        /// <summary>
        /// Метод неправильного ввода логина
        /// </summary>
        private void InCorrectLogin()
        {
            if (LNameUser.Content != "")
            {
                LNameUser.Content = "";
                DoubleAnimation StartAnimation = new DoubleAnimation();
                StartAnimation.From = 1;
                StartAnimation.To = 0;
                StartAnimation.Duration = TimeSpan.FromSeconds(0.6);

                StartAnimation.Completed += delegate
                {
                    IUser.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic-user.png"));
                    DoubleAnimation EndAnimation = new DoubleAnimation();
                    EndAnimation.From = 0;
                    EndAnimation.To = 1;
                    EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                    IUser.BeginAnimation(OpacityProperty, EndAnimation);
                };

                IUser.BeginAnimation(OpacityProperty, StartAnimation);
            }

            if (TbLogin.Text.Length > 0)
                SetNotification("Login is incorrect", Brushes.Red);
        }

        /// <summary>
        /// Метод успешного ввода капчи
        /// </summary>
        private void CorrectCapture()
        {
            // Отключаем элемент капчи
            Capture.IsEnabled = false;
            // Запоминаем что ввод капчи осуществлён
            IsCapture = true;
            // Вызываем генерацию нового пароля
            SendNewPassword();
        }

        /// <summary>
        /// Метод ввода логина
        /// </summary>
        private void SetLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);
        }

        /// <summary>
        /// Метод ввода логина
        /// </summary>
        private void SetLogin(object sender, RoutedEventArgs e) =>
            MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);

        /// <summary>
        /// Метод создания нового пароля
        /// </summary>
        public void SendNewPassword()
        {
            if (IsCapture)
            {
                if (MainWindow.mainWindow.UserLogIn.Password != String.Empty)
                {
                    DoubleAnimation StartAnimation = new DoubleAnimation();
                    StartAnimation.From = 1;
                    StartAnimation.To = 0;
                    StartAnimation.Duration = TimeSpan.FromSeconds(0.6);

                    StartAnimation.Completed += delegate
                    {
                        IUser.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic-mail.png"));
                        DoubleAnimation EndAnimation = new DoubleAnimation();
                        EndAnimation.From = 0;
                        EndAnimation.To = 1;
                        EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                        IUser.BeginAnimation(OpacityProperty, EndAnimation);
                    };

                    IUser.BeginAnimation(OpacityProperty, StartAnimation);
                    SetNotification("An email has been sent to your email.", Brushes.Black);
                    MainWindow.mainWindow.UserLogIn.CrateNewPassword();
                }
            }
        }

        

        /// <summary>
        /// Открытие страницы логина
        /// </summary>
        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
