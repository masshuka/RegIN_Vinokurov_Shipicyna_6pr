using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Regin_New.Classes;
using Aspose.Imaging;
using Image = Aspose.Imaging.Image;

namespace Regin_New.Pages
{
    /// <summary>
    /// Логика взаимодействия для Regin.xaml
    /// </summary>
    public partial class Regin : Page
    {
        /// <summary>
        /// Файловый диалог
        /// </summary>
        OpenFileDialog FileDialogImage = new OpenFileDialog();

        /// <summary>
        /// Проверка на ввод логина
        /// </summary>
        bool BCorrectLogin = false;

        /// <summary>
        /// Проверка на ввод пароля
        /// </summary>
        bool BCorrectPassword = false;

        /// <summary>
        /// Проверка на подтверждение пароля
        /// </summary>
        bool BCorrectConfirmPassword = false;

        /// <summary>
        /// Проверка на указания изображения
        /// </summary>
        bool BSetImages = false;

        public Regin()
        {
            InitializeComponent();
            MainWindow.mainWindow.UserLogIn.HandlerCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.HandlerInCorrectLogin += InCorrectLogin;
            FileDialogImage.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg";
            FileDialogImage.RestoreDirectory = true;
            FileDialogImage.Title = "Choose a photo for your avatar";
        }

        /// <summary>
        /// Функция правильно введённого логина
        /// </summary>
        private void CorrectLogin()
        {
            SetNotification("Login already in use", Brushes.Red);
            BCorrectLogin = false;
        }

        /// <summary>
        /// Функция не правильно введённого логина
        /// </summary>
        private void InCorrectLogin() => SetNotification("", Brushes.Black);

        private void SetLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SetLogin();
        }

        /// <summary>
        /// Метод ввода логина
        /// </summary>
        private void SetLogin(object sender, RoutedEventArgs e) =>
            SetLogin();

        /// <summary>
        /// Метод ввода логина
        /// </summary>
        public void SetLogin()
        {
            Regex regex = new Regex(@"([a-zA-Z0-9._-]{4,}@[a-zA-Z0-9._-]{2,}\.[a-zA-Z0-9._-]{2,})");
            BCorrectLogin = regex.IsMatch(TbLogin.Text);

            if (BCorrectLogin)
            {
                SetNotification("", Brushes.Black);
                MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);
            }
            else
                SetNotification("Invalid login", Brushes.Red);

            OnRegin();
        }

        /// <summary>
        /// Метод ввода пароля
        /// </summary>
        private void SetPassword(object sender, RoutedEventArgs e) =>
            SetPassword();

        /// <summary>
        /// Метод ввода пароля
        /// </summary>
        private void SetPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SetPassword();
        }

        /// <summary>
        /// Метод ввода пароля
        /// </summary>
        public void SetPassword()
        {
            Regex regex = new Regex(@"(?=.*[0-9])(?=.*[!@#$%^&?*\-_=])(?=.*[a-z])(?=.*[A-Z])[0-9a-zA-Z!@#$%^&?*\-_=]{10,}");
            // (?=.*[0-9]) - строка содержит хотя бы одно число;
            // (?=.*[!@#$%^&?*\-_=]) - строка содержит хотя бы один спецсимвол;
            // (?=.*[a-z]) - строка содержит хотя бы одну латинскую букву в нижнем регистре;
            // (?=.*[A-Z]) - строка содержит хотя бы одну латинскую букву в верхнем регистре;
            // [0-9a-zA-Z!@#$%^&?*\-_=]{10,} - строка состоит не менее, чем из 10 вышеупомянутых символов.

            BCorrectPassword = regex.IsMatch(PbPassword.Password);

            if (BCorrectPassword)
            {
                SetNotification("", Brushes.Black);
                if (PbConfirmPassword.Password.Length > 0)
                    ConfirmPassword(true);
                OnRegin();
            }
            else
                SetNotification("Invalid password", Brushes.Red);
        }

        /// <summary>
        /// Метод повторного ввода пароля
        /// </summary>
        private void ConfirmPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ConfirmPassword();
        }

        /// <summary>
        /// Метод повторного ввода пароля
        /// </summary>
        private void ConfirmPassword(object sender, RoutedEventArgs e) =>
            ConfirmPassword();

        /// <summary>
        /// Метод повторного ввода пароля
        /// </summary>
        public void ConfirmPassword(bool Pass = false)
        {
            BCorrectConfirmPassword = PbConfirmPassword.Password == PbPassword.Password;

            if (!BCorrectConfirmPassword)
                SetNotification("Passwords do not match", Brushes.Red);
            else
            {
                SetNotification("", Brushes.Black);
                if (!Pass)
                    SetPassword();
            }
        }

        /// <summary>
        /// Метод регистрации
        /// </summary>
        void OnRegin()
        {
            if (!BCorrectLogin)
                return;
            if (TbName.Text.Length == 0)
                return;
            if (!BCorrectPassword)
                return;
            if (!BCorrectConfirmPassword)
                return;

            MainWindow.mainWindow.UserLogIn.Login = TbLogin.Text;
            MainWindow.mainWindow.UserLogIn.Password = PbPassword.Password;
            MainWindow.mainWindow.UserLogIn.Name = TbName.Text;

            if (BSetImages)
                MainWindow.mainWindow.UserLogIn.Image = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\IUser.jpg");

            MainWindow.mainWindow.UserLogIn.DateUpdate = DateTime.Now;
            MainWindow.mainWindow.UserLogIn.DateCreate = DateTime.Now;
            MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Regin));
        }

        /// <summary>
        /// Ввод букв
        /// </summary>
        private void SetName(object sender, TextCompositionEventArgs e)
        {
            // Проверяем что символ относится к категории букв
            e.Handled = !(Char.IsLetter(e.Text, 0));
        }

        public void SetNotification(string Message, SolidColorBrush _Color)
        {
            LNameUser.Content = Message;
            LNameUser.Foreground = _Color;
        }

        private void SelectImage(object sender, MouseButtonEventArgs e)
        {
            if (FileDialogImage.ShowDialog() == true)
            {
                ProcessSelectedImage();
                BSetImages = true;
            }
            else
                BSetImages = false;
        }

        private void ProcessSelectedImage()
        {
            string fileName = "IUser.jpg";
            string filePath = Directory.GetCurrentDirectory() + @"\" + fileName;

            // Загружаем и изменяем размер изображения
            using (var image = Image.Load(FileDialogImage.FileName))
            {
                int NewWidth = 0;
                int NewHeight = 0;

                if (image.Width > image.Height)
                {
                    NewWidth = (int)(image.Width * (256f / image.Height));
                    NewHeight = 256;
                }
                else
                {
                    NewWidth = 256;
                    NewHeight = (int)(image.Height * (256f / image.Width));
                }

                image.Resize(NewWidth, NewHeight);
                image.Save(filePath);
            }

            // Обрезаем изображение до квадрата
            using (var rasterImage = (RasterImage)RasterImage.Load(filePath))
            {
                if (!rasterImage.IsCached)
                {
                    rasterImage.CacheData();
                }

                int X = 0;
                int Y = 0;

                if (rasterImage.Width > rasterImage.Height)
                    X = (int)((rasterImage.Width - 256f) / 2);
                else
                    Y = (int)((rasterImage.Height - 256f) / 2);

                var rectangle = new Aspose.Imaging.Rectangle(X, Y, 256, 256);
                rasterImage.Crop(rectangle);
                rasterImage.Save(filePath);
            }

            ShowImageWithAnimation(filePath);
        }

        private void ShowImageWithAnimation(string filePath)
        {
            DoubleAnimation StartAnimation = new DoubleAnimation();
            StartAnimation.From = 1;
            StartAnimation.To = 0;
            StartAnimation.Duration = TimeSpan.FromSeconds(0.6);

            StartAnimation.Completed += delegate
            {
                IUser.Source = new BitmapImage(new Uri(filePath));
                DoubleAnimation EndAnimation = new DoubleAnimation();
                EndAnimation.From = 0;
                EndAnimation.To = 1;
                EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                IUser.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, EndAnimation);
            };

            IUser.BeginAnimation(System.Windows.Controls.Image.OpacityProperty, StartAnimation);
        }

        /// <summary>
        /// Метод перехода на авторизацию
        /// </summary>
        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
