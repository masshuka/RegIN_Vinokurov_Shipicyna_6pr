using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Regin_New.Classes;

namespace Regin_New.Pages
{
    /// <summary>
    /// Логика взаимодействия для Confirmation.xaml
    /// </summary>
    public partial class Confirmation : Page
    {
        public Confirmation(TypeConfirmation TypeConfirmation)
        {
            InitializeComponent();
            ThisTypeConfirmation = TypeConfirmation;
            SendMailCode();
        }

        /// <summary>
        /// Тип перечисления для чего используется подтверждение
        /// </summary>
        public enum TypeConfirmation
        {
            Login,
            Regin
        }

        /// Для его используется подтверждение
        /// </summary>
        TypeConfirmation ThisTypeConfirmation;

        /// <summary>
        /// Код отправленный на почту пользователя
        /// </summary>
        public int Code = 0;


        /// <summary>
        /// Метод отправки сообщения на почту
        /// </summary>
        public void SendMailCode()
        {
            Code = new Random().Next(100000, 999999);
            Classes.SendMail.SendMessage($"Login code: {Code}", MainWindow.mainWindow.UserLogIn.Login);
            Thread TSendMailCode = new Thread(TimerSendMailCode);
            TSendMailCode.Start();
        }

        /// <summary>
        /// Ожидание отправки нового кода
        /// </summary>
        public void TimerSendMailCode()
        {
            for (int i = 0; i < 60; i++)
            {
                Dispatcher.Invoke(() =>
                {
                    lTimer.Content = $"A second message can be sent after {60 - i} seconds";
                });
                Thread.Sleep(1000);
            }

            Dispatcher.Invoke(() =>
            {
                BSendMessage.IsEnabled = true;
                lTimer.Content = "";
            });
        }

        /// <summary>
        /// Отправка кода подтверждения на почту пользователя
        /// </summary>
        private void SendMail(object sender, RoutedEventArgs e)
        {
            // Вызываем метод отправки сообщения на почту пользователя
            SendMailCode();
        }

        /// <summary>
        /// Вызов метода проверки отправленного кода на почту и введённого пользователем
        /// </summary>
        private void SetCode(object sender, KeyEventArgs e)
        {
            if (TbCode.Text.Length == 6)
                SetCode();
        }

        /// <summary>
        /// Вызов метода проверки отправленного кода на почту и введённого пользователем
        /// </summary>
        private void SetCode(object sender, RoutedEventArgs e) =>
            SetCode();

        /// <summary>
        /// Метод проверки отправленного кода на почту и введённого пользователем
        /// </summary>
        void SetCode()
        {
            if (TbCode.Text == Code.ToString() && TbCode.IsEnabled == true)
            {
                TbCode.IsEnabled = false;
                if (ThisTypeConfirmation == TypeConfirmation.Login)
                    MessageBox.Show("Авторизация пользователя успешно подтверждена.");
                else
                {
                    MainWindow.mainWindow.UserLogIn.SetUser();
                    MessageBox.Show("Регистрация пользователя успешно подтверждена.");
                }
            }
        }

        /// <summary>
        /// Метод открытия страницы авторизации
        /// </summary>
        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
