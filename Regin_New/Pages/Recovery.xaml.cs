using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Regin_New.Pages
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
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            MainWindow.mainWindow.UserLogIn.HandlerCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.HandlerInCorrectLogin += InCorrectLogin;
            Capture.HandlerCorrectCapture += CorrectCapture;
        }

        public void SetNotification(string Message, SolidColorBrush _Color)
        {
            LNameUser.Content = Message;
            LNameUser.Foreground = _Color;
        }

        private void CorrectLogin()
        {
            if (OldLogin == TbLogin.Text) return;

            SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);
            UpdateUserImage();
            OldLogin = TbLogin.Text;

            if (IsCapture)
                SendNewPassword();
        }

        private void UpdateUserImage()
        {
            try
            {
                if (MainWindow.mainWindow.UserLogIn.Image != null &&
                    MainWindow.mainWindow.UserLogIn.Image.Length > 0)
                {
                    var bitmapImage = CreateBitmapImage(MainWindow.mainWindow.UserLogIn.Image);
                    AnimateImageChange(bitmapImage);
                }
                else
                {
                    SetDefaultUserImage();
                }
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
                SetDefaultUserImage();
            }
        }

        private BitmapImage CreateBitmapImage(byte[] imageData)
        {
            var bitmapImage = new BitmapImage();
            using (var ms = new MemoryStream(imageData))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }

        private void AnimateImageChange(BitmapImage newImage)
        {
            var fadeOut = CreateAnimation(1, 0, 0.6);
            fadeOut.Completed += (s, e) =>
            {
                IUser.Source = newImage;
                var fadeIn = CreateAnimation(0, 1, 1.2);
                IUser.BeginAnimation(Image.OpacityProperty, fadeIn);
            };
            IUser.BeginAnimation(Image.OpacityProperty, fadeOut);
        }

        private void InCorrectLogin()
        {
            if (!string.IsNullOrEmpty(LNameUser.Content?.ToString()))
            {
                LNameUser.Content = "";
                SetDefaultUserImage();
            }

            if (TbLogin.Text.Length > 0)
                SetNotification("Login is incorrect", Brushes.Red);
        }

        private void SetDefaultUserImage()
        {
            var fadeOut = CreateAnimation(1, 0, 0.6);
            fadeOut.Completed += (s, e) =>
            {
                IUser.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic-user.png"));
                var fadeIn = CreateAnimation(0, 1, 1.2);
                IUser.BeginAnimation(OpacityProperty, fadeIn);
            };
            IUser.BeginAnimation(OpacityProperty, fadeOut);
        }

        private DoubleAnimation CreateAnimation(double from, double to, double seconds)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(seconds)
            };
        }

        private void CorrectCapture()
        {
            Capture.IsEnabled = false;
            IsCapture = true;
            SendNewPassword();
        }

        private void SetLogin(KeyEventArgs e = null, RoutedEventArgs re = null)
        {
            if (e?.Key == Key.Enter || re != null)
                MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);
        }

        private void SetLogin(object sender, KeyEventArgs e) => SetLogin(e);
        private void SetLogin(object sender, RoutedEventArgs e) => SetLogin(null, e);

        public void SendNewPassword()
        {
            if (!IsCapture || string.IsNullOrEmpty(MainWindow.mainWindow.UserLogIn.Password))
                return;

            ShowMailSentAnimation();
            SetNotification("An email has been sent to your email.", Brushes.Black);
            MainWindow.mainWindow.UserLogIn.CrateNewPassword();
        }

        private void ShowMailSentAnimation()
        {
            var fadeOut = CreateAnimation(1, 0, 0.6);
            fadeOut.Completed += (s, e) =>
            {
                IUser.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic-mail.png"));
                var fadeIn = CreateAnimation(0, 1, 1.2);
                IUser.BeginAnimation(OpacityProperty, fadeIn);
            };
            IUser.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void OpenLogin(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new Login());
    }
}
