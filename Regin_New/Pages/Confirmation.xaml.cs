using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Regin_New.Pages
{
    public partial class Confirmation : Page
    {
        private readonly TypeConfirmation confirmationType;
        private int Code = 0;
        private const int CountdownSeconds = 60;

        public enum TypeConfirmation { Login, Regin }

        public Confirmation(TypeConfirmation typeConfirmation)
        {
            InitializeComponent();
            confirmationType = typeConfirmation;
            SendMailCode();
        }

        public void SendMailCode()
        {
            Code = new Random().Next(100000, 999999);
            Classes.SendMail.SendMessage($"Login code: {Code}", MainWindow.mainWindow.UserLogIn.Login);
            BSendMessage.IsEnabled = false;

            Thread timerThread = new Thread(TimerSendMailCode);
            timerThread.Start();
        }

        public void TimerSendMailCode()
        {
            for (int i = CountdownSeconds; i > 0; i--)
            {
                Dispatcher.Invoke(() =>
                {
                    lTimer.Content = $"A second message can be sent after {i} seconds";
                });
                Thread.Sleep(1000);
            }

            Dispatcher.Invoke(() =>
            {
                BSendMessage.IsEnabled = true;
                lTimer.Content = "";
            });
        }

        private void SendMail(object sender, RoutedEventArgs e)
        {
            SendMailCode();
        }

        private void SetCode(object sender, KeyEventArgs e)
        {
            if (TbCode.Text.Length == 6)
                ValidateCode();
        }

        private void SetCode(object sender, RoutedEventArgs e)
        {
            ValidateCode();
        }

        private void ValidateCode()
        {
            if (TbCode.Text != Code.ToString() || !TbCode.IsEnabled)
                return;

            TbCode.IsEnabled = false;

            if (confirmationType == TypeConfirmation.Login)
            {
                MessageBox.Show("Authorization confirmed successfully.");

                if (string.IsNullOrEmpty(MainWindow.mainWindow.UserLogIn.PinCode))
                {
                    var result = MessageBox.Show("Set up a 4-digit PIN for quick login?",
                        "Quick Login", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        MainWindow.mainWindow.OpenPage(new SetPinCode());
                        return;
                    }
                }
            }
            else
            {
                MainWindow.mainWindow.UserLogIn.SetUser();
                MessageBox.Show("Registration confirmed successfully.");

                var result = MessageBox.Show("Set up a 4-digit PIN for quick login?",
                    "Quick Login", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    MainWindow.mainWindow.OpenPage(new SetPinCode());
                }
                else
                {
                    MainWindow.mainWindow.OpenPage(new Login());
                }
            }
        }

        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}