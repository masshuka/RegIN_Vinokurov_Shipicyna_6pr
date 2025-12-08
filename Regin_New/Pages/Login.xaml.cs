using Regin_New.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Regin_New.Pages
{
    public partial class Login : Page
    {
        string OldLogin;
        int CountSetPassword = 2;
        bool IsCapture = false;
        private const int BlockTimeSeconds = 180;

        public Login()
        {
            InitializeComponent();
            SubscribeToEvents();
            AddPinCodeSwitch();
        }

        private void SubscribeToEvents()
        {
            MainWindow.mainWindow.UserLogIn.HandlerCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.HandlerInCorrectLogin += InCorrectLogin;
            Capture.HandlerCorrectCapture += CorrectCapture;
        }

        public void SetNotification(string message, SolidColorBrush color)
        {
            Username.Content = message;
            Username.Foreground = color;
        }

        public void InCorrectLogin()
        {
            if (!string.IsNullOrEmpty(Username.Content?.ToString()))
            {
                Username.Content = "";
                AnimateImageChange("pack://application:,,,/Images/ic-user.png");
            }

            if (TbLogin.Text.Length > 0)
                SetNotification("Login is incorrect", Brushes.Red);
        }

        private void SetLogin(KeyEventArgs e = null, RoutedEventArgs re = null)
        {
            if (e?.Key == Key.Enter || re != null)
            {
                MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);
                if (TbPassword.Password.Length > 0)
                    SetPassword();
            }
        }

        private void SetLogin(object sender, KeyEventArgs e) => SetLogin(e);
        private void SetLogin(object sender, RoutedEventArgs e) => SetLogin(null, e);

        public void CorrectLogin()
        {
            if (OldLogin == TbLogin.Text) return;

            SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);

            if (MainWindow.mainWindow.UserLogIn.Image == null ||
                MainWindow.mainWindow.UserLogIn.Image.Length == 0)
            {
                SetDefaultImage();
                OldLogin = TbLogin.Text;
                return;
            }

            LoadUserImage();
            OldLogin = TbLogin.Text;
        }

        private void LoadUserImage()
        {
            try
            {
                var bitmapImage = CreateBitmapImage(MainWindow.mainWindow.UserLogIn.Image);
                AnimateImageChange(bitmapImage);
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
                SetDefaultImage();
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
                User.Source = newImage;
                var fadeIn = CreateAnimation(0, 1, 1.2);
                User.BeginAnimation(Image.OpacityProperty, fadeIn);
            };
            User.BeginAnimation(Image.OpacityProperty, fadeOut);
        }

        private void AnimateImageChange(string imageUri)
        {
            var fadeOut = CreateAnimation(1, 0, 0.6);
            fadeOut.Completed += (s, e) =>
            {
                User.Source = new BitmapImage(new Uri(imageUri));
                var fadeIn = CreateAnimation(0, 1, 1.2);
                User.BeginAnimation(OpacityProperty, fadeIn);
            };
            User.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void SetDefaultImage()
        {
            User.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic-user.png"));
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

        public void CorrectCapture()
        {
            Capture.IsEnabled = false;
            IsCapture = true;
        }

        private void SetPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ValidatePassword();
        }

        public void SetPassword()
        {
            ValidatePassword();
        }

        private void ValidatePassword()
        {
            if (string.IsNullOrEmpty(MainWindow.mainWindow.UserLogIn.Password))
                return;

            if (!IsCapture)
            {
                SetNotification("Enter capture", Brushes.Red);
                return;
            }

            if (MainWindow.mainWindow.UserLogIn.Password == TbPassword.Password)
            {
                MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Login));
            }
            else
            {
                HandleIncorrectPassword();
            }
        }

        private void HandleIncorrectPassword()
        {
            if (CountSetPassword > 0)
            {
                SetNotification($"Password is incorrect, {CountSetPassword} attempts left", Brushes.Red);
                CountSetPassword--;
            }
            else
            {
                new Thread(BlockAuthorization).Start();
            }

            SendMail.SendMessage("An attempt was made to log into your account.",
                MainWindow.mainWindow.UserLogIn.Login);
        }

        public void BlockAuthorization()
        {
            var endTime = DateTime.Now.AddMinutes(3);
            SetControlsEnabled(false);

            for (int i = 0; i < BlockTimeSeconds; i++)
            {
                UpdateBlockTimer(endTime);
                Thread.Sleep(1000);
            }

            ResetAuthorizationState();
        }

        private void SetControlsEnabled(bool enabled)
        {
            Dispatcher.Invoke(() =>
            {
                TbLogin.IsEnabled = enabled;
                TbPassword.IsEnabled = enabled;
                Capture.IsEnabled = enabled;
            });
        }

        private void UpdateBlockTimer(DateTime endTime)
        {
            Dispatcher.Invoke(() =>
            {
                var remaining = endTime - DateTime.Now;
                SetNotification($"Reauthorization available in: {remaining:mm\\:ss}", Brushes.Red);
            });
        }

        private void ResetAuthorizationState()
        {
            Dispatcher.Invoke(() =>
            {
                SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);
                TbLogin.IsEnabled = true;
                TbPassword.IsEnabled = true;
                Capture.IsEnabled = true;
                Capture.CreateCapture();
                IsCapture = false;
                CountSetPassword = 2;
            });
        }

        private void RecoveryPassword(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new Recovery());

        private void OpenRegIn(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new Regin());

        private void AddPinCodeSwitch()
        {
            var switchPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 400, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            var switchLabel = new Label
            {
                Content = "Use PIN code",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(31, 146, 181))
            };

            var toggleButton = new CheckBox
            {
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            toggleButton.Checked += (s, e) => SwitchToPinCodeMode();
            toggleButton.Unchecked += (s, e) => SwitchToPasswordMode();

            switchPanel.Children.Add(switchLabel);
            switchPanel.Children.Add(toggleButton);

            var grid = (Grid)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this, 0), 0);
            grid.Children.Add(switchPanel);
        }

        private void SwitchToPinCodeMode()
        {
            TbPassword.Visibility = Visibility.Collapsed;
            var passwordLabel = ((Grid)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this, 0), 0))
                .Children.OfType<Label>().FirstOrDefault(l => l.Content?.ToString() == "Enter password:");
            if (passwordLabel != null)
                passwordLabel.Visibility = Visibility.Collapsed;

            var pinBox = new PasswordBox
            {
                Name = "PinCodeBox",
                Margin = new Thickness(10, 342, 10, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Height = 26,
                MaxLength = 4
            };
            pinBox.KeyUp += PinCodeBox_KeyUp;

            var grid = (Grid)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this, 0), 0);
            grid.Children.Add(pinBox);
        }

        private void SwitchToPasswordMode()
        {
            TbPassword.Visibility = Visibility.Visible;
            var passwordLabel = ((Grid)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this, 0), 0))
                .Children.OfType<Label>().FirstOrDefault(l => l.Content?.ToString() == "Enter password:");
            if (passwordLabel != null)
                passwordLabel.Visibility = Visibility.Visible;

            var grid = (Grid)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this, 0), 0);
            var pinBox = grid.Children.OfType<PasswordBox>().FirstOrDefault(p => p.Name == "PinCodeBox");
            if (pinBox != null)
                grid.Children.Remove(pinBox);
        }

        private void PinCodeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AuthenticateWithPinCode();
            }
        }

        private void AuthenticateWithPinCode()
        {
            var grid = (Grid)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this, 0), 0);
            var pinBox = grid.Children.OfType<PasswordBox>().FirstOrDefault(p => p.Name == "PinCodeBox");

            if (pinBox == null || pinBox.Password.Length != 4)
            {
                SetNotification("Enter 4-digit PIN", Brushes.Red);
                return;
            }

            if (MainWindow.mainWindow.UserLogIn.PinCode == pinBox.Password)
            {
                MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Login));
            }
            else
            {
                SetNotification("Invalid PIN", Brushes.Red);
                pinBox.Password = "";
            }
        }
    }
}