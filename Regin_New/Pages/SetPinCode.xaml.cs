using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Regin_New.Pages
{
    public partial class SetPinCode : Page
    {
        private bool pinCodeSet = false;

        public SetPinCode()
        {
            InitializeComponent();
        }

        private void PinCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void PinCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PinCodeTextBox.Text.Length == 4 && !pinCodeSet)
            {
                // Устанавливаем пин-код в объект пользователя
                MainWindow.mainWindow.UserLogIn.PinCode = PinCodeTextBox.Text;

                // Сохраняем в БД
                bool saved = SavePinCodeToDatabase();

                if (saved)
                {
                    pinCodeSet = true;
                    MessageBox.Show($"Пин код '{PinCodeTextBox.Text}' сохранен");
                    MainWindow.mainWindow.OpenPage(new Login());
                }
                else
                {
                    MessageBox.Show("Ошибка сохранения PIN-кода.");
                    pinCodeSet = false;
                    PinCodeTextBox.Text = "";
                    PinCodeTextBox.Focus();
                }
            }
            else if (PinCodeTextBox.Text.Length > 4)
            {
                PinCodeTextBox.Text = PinCodeTextBox.Text.Substring(0, 4);
                PinCodeTextBox.CaretIndex = 4;
            }
        }

        private bool SavePinCodeToDatabase()
        {
            try
            {
                var connection = Classes.WorkingDB.OpenConnection();
                if (connection == null)
                {
                    MessageBox.Show("Database connection error");
                    return false;
                }

                using (var cmd = new MySqlCommand(
                    "UPDATE users SET PinCode = @PinCode, DateUpdate = @DateUpdate WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@PinCode", MainWindow.mainWindow.UserLogIn.PinCode);
                    cmd.Parameters.AddWithValue("@DateUpdate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Id", MainWindow.mainWindow.UserLogIn.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    Classes.WorkingDB.CloseConnection(connection);

                    if (rowsAffected > 0)
                    {
                        // Обновляем дату в объекте
                        MainWindow.mainWindow.UserLogIn.DateUpdate = DateTime.Now;
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"No rows affected. User ID: {MainWindow.mainWindow.UserLogIn.Id}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения PIN-кода: {ex.Message}");
                return false;
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы сможете установить PIN-код позже. Пока используйте пароль.");
            MainWindow.mainWindow.OpenPage(new Login());
        }

        private void SetPinCode_Loaded(object sender, RoutedEventArgs e)
        {
            // Фокус на поле ввода при загрузке
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                new Action(() => PinCodeTextBox.Focus()));

            // Проверяем ID пользователя
            if (MainWindow.mainWindow.UserLogIn.Id <= 0)
            {
                MessageBox.Show("User not found. Please log in again.");
                MainWindow.mainWindow.OpenPage(new Login());
            }
        }
    }
}