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
        private bool _isUsingPinCode = false;

        public Login()
        {
            InitializeComponent();
            SubscribeToEvents();

            // Скрываем кнопку Use PIN-code пока не загружен пользователь
            // Найдем кнопку Use PIN-code по имени
            var usePinCodeLabel = FindVisualChild<Label>(this, l => l.Content?.ToString() == "Use PIN-code");
            if (usePinCodeLabel != null)
                usePinCodeLabel.Visibility = Visibility.Collapsed;
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

            // Скрываем кнопку Use PIN-code при неверном логине
            var usePinCodeLabel = FindVisualChild<Label>(this, l => l.Content?.ToString() == "Use PIN-code");
            if (usePinCodeLabel != null)
                usePinCodeLabel.Visibility = Visibility.Collapsed;

            // Скрываем PIN-контейнер если он был открыт
            PinCodeContainer.Visibility = Visibility.Collapsed;
            PasswordLabel.Visibility = Visibility.Visible;
            TbPassword.Visibility = Visibility.Visible;
        }

        private void SetLogin(KeyEventArgs e = null, RoutedEventArgs re = null)
        {
            if (e?.Key == Key.Enter || re != null)
            {
                MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);
                if (TbPassword.Password.Length > 0 && !_isUsingPinCode)
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
            }
            else
            {
                LoadUserImage();
                OldLogin = TbLogin.Text;
            }

            // Показываем кнопку Use PIN-code только если у пользователя есть PIN
            UpdateUsePinCodeButtonVisibility();
        }

        private void UpdateUsePinCodeButtonVisibility()
        {
            var usePinCodeLabel = FindVisualChild<Label>(this, l => l.Content?.ToString() == "Use PIN-code");
            if (usePinCodeLabel != null)
            {
                // Проверяем, есть ли у пользователя PIN-код
                if (!string.IsNullOrEmpty(MainWindow.mainWindow.UserLogIn.PinCode))
                {
                    usePinCodeLabel.Visibility = Visibility.Visible;

                    // Убираем старый обработчик и добавляем новый
                    usePinCodeLabel.MouseDown -= RecoveryPassword; // Удаляем старый
                    usePinCodeLabel.MouseDown += SwitchToPinCode;   // Добавляем новый
                }
                else
                {
                    usePinCodeLabel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SwitchToPinCode(object sender, MouseButtonEventArgs e)
        {
            _isUsingPinCode = true;

            // Скрываем пароль, показываем PIN
            PasswordLabel.Visibility = Visibility.Collapsed;
            TbPassword.Visibility = Visibility.Collapsed;
            PinCodeContainer.Visibility = Visibility.Visible;

            // Фокусируемся на поле PIN
            if (PinCodeBox != null)
            {
                PinCodeBox.Focus();
            }

            // Скрываем кнопку Use PIN-code
            var usePinCodeLabel = FindVisualChild<Label>(this, l => l.Content?.ToString() == "Use PIN-code");
            if (usePinCodeLabel != null)
                usePinCodeLabel.Visibility = Visibility.Collapsed;
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
                if (PinCodeBox != null) PinCodeBox.IsEnabled = true;

                // Восстанавливаем режим пароля
                _isUsingPinCode = false;
                PinCodeContainer.Visibility = Visibility.Collapsed;
                PasswordLabel.Visibility = Visibility.Visible;
                TbPassword.Visibility = Visibility.Visible;

                // Показываем кнопку Use PIN-code если есть PIN
                UpdateUsePinCodeButtonVisibility();
            });
        }

        private void RecoveryPassword(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new Recovery());

        private void OpenRegIn(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new Regin());

        private void PinCodeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && PinCodeBox.Password.Length == 4)
            {
                AuthenticateWithPinCode();
            }
        }

        private void AuthenticateWithPinCode()
        {
            if (PinCodeBox == null || PinCodeBox.Password.Length != 4)
            {
                SetNotification("Введите 4-значный PIN-код", Brushes.Red);
                return;
            }

            Debug.WriteLine($"Введенный PIN: {PinCodeBox.Password}");
            Debug.WriteLine($"PIN в объекте пользователя: {MainWindow.mainWindow.UserLogIn.PinCode}");

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

        // Вспомогательные методы для поиска элементов
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