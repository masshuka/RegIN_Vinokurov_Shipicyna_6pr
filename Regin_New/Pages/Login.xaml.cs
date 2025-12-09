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
        private PasswordBox PinCodeBox;
        private CheckBox PinCodeToggle;
        private StackPanel SwitchPanel;

        public Login()
        {
            InitializeComponent();
            SubscribeToEvents();
            InitializePinCodeControls();
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

            SendMail.SendMessage("Была предпринята попытка входа в Вашу учетную запись.",
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
                if (PinCodeToggle != null) PinCodeToggle.IsEnabled = enabled;
                if (PinCodeBox != null) PinCodeBox.IsEnabled = enabled;
            });
        }

        private void UpdateBlockTimer(DateTime endTime)
        {
            Dispatcher.Invoke(() =>
            {
                var remaining = endTime - DateTime.Now;
                SetNotification($"Повторная авторизация доступна в: {remaining:mm\\:ss}", Brushes.Red);
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
                if (PinCodeToggle != null) PinCodeToggle.IsEnabled = true;
                if (PinCodeBox != null) PinCodeBox.IsEnabled = true;
            });
        }
        private void RecoveryPassword(object sender, MouseButtonEventArgs e) =>
                    MainWindow.mainWindow.OpenPage(new Recovery());

        private void OpenRegIn(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new Regin());

        private void OpenPinCode(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new SetPinCode(true));
        }

        private void InitializePinCodeControls()
        {
            SwitchPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 375, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            var switchLabel = new Label
            {
                Content = "Use PIN code",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(31, 146, 181))
            };

            PinCodeToggle = new CheckBox
            {
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            PinCodeToggle.Checked += (s, e) => SwitchToPinCodeMode();
            PinCodeToggle.Unchecked += (s, e) => SwitchToPasswordMode();

            SwitchPanel.Children.Add(switchLabel);
            SwitchPanel.Children.Add(PinCodeToggle);

            var mainGrid = FindVisualChild<Grid>(this);
            if (mainGrid != null)
            {
                mainGrid.Children.Add(SwitchPanel);
            }
        }

        private void SwitchToPinCodeMode()
        {
            TbPassword.Visibility = Visibility.Collapsed;
            var passwordLabel = FindVisualChild<Label>(this, l => l.Content?.ToString() == "Enter password:");
            if (passwordLabel != null)
                passwordLabel.Visibility = Visibility.Collapsed;

            PinCodeBox = new PasswordBox
            {
                Name = "PinCodeBox",
                Margin = new Thickness(10, 342, 10, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Height = 26,
                MaxLength = 4
            };
            PinCodeBox.KeyUp += PinCodeBox_KeyUp;

            var mainGrid = FindVisualChild<Grid>(this);
            if (mainGrid != null)
            {
                mainGrid.Children.Add(PinCodeBox);
            }
        }

        private void SwitchToPasswordMode()
        {
            TbPassword.Visibility = Visibility.Visible;
            var passwordLabel = FindVisualChild<Label>(this, l => l.Content?.ToString() == "Enter password:");
            if (passwordLabel != null)
                passwordLabel.Visibility = Visibility.Visible;

            var mainGrid = FindVisualChild<Grid>(this);
            if (mainGrid != null && PinCodeBox != null)
            {
                mainGrid.Children.Remove(PinCodeBox);
                PinCodeBox = null;
            }
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
            if (PinCodeBox == null || PinCodeBox.Password.Length != 4)
            {
                SetNotification("Enter 4-digit PIN", Brushes.Red);
                return;
            }

            if (MainWindow.mainWindow.UserLogIn.PinCode == PinCodeBox.Password)
            {
                MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Login));
            }
            else
            {
                SetNotification("Invalid PIN", Brushes.Red);
                PinCodeBox.Password = "";
            }
        }
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }

        private T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && predicate(result))
                    return result;

                var childResult = FindVisualChild(child, predicate);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }
    }
}