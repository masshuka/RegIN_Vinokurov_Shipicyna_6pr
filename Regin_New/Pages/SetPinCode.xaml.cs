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
        private readonly bool isLoginMode;

        public SetPinCode(bool isLoginMode = false)
        {
            InitializeComponent();
            this.isLoginMode = isLoginMode;
            InitializeTexts();
        }

        private void InitializeTexts()
        {
            if (isLoginMode)
            {
                TitleTextBlock.Text = "Введите 4-значный пин-код";
                DescriptionTextBlock.Text = "Введите ваш PIN-код для авторизации";
            }
            else
            {
                TitleTextBlock.Text = "Установите 4-значный пин-код";
                DescriptionTextBlock.Text = "Пин-код будет использоваться для авторизации";
            }
        }

        private void PinCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void PinCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PinCodeTextBox.Text.Length == 4 && !pinCodeSet)
            {
                if (isLoginMode)
                {
                    bool isValid = CheckPinCode();

                    if (isValid)
                    {
                        pinCodeSet = true;
                        MessageBox.Show("Авторизация успешна.");
                        MainWindow.mainWindow.OpenPage(new Login());
                    }
                    else
                    {
                        MessageBox.Show("Неверный PIN-код.");
                        pinCodeSet = false;
                        PinCodeTextBox.Text = "";
                        PinCodeTextBox.Focus();
                    }
                }
                else
                {
                    MainWindow.mainWindow.UserLogIn.PinCode = PinCodeTextBox.Text;
                    bool saved = SavePinCodeToDatabase();

                    if (saved)
                    {
                        pinCodeSet = true;
                        MessageBox.Show("PIN-код успешно установлен.");
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
            }
            else if (PinCodeTextBox.Text.Length > 4)
            {
                PinCodeTextBox.Text = PinCodeTextBox.Text.Substring(0, 4);
                PinCodeTextBox.CaretIndex = 4;
            }
        }

        private bool CheckPinCode()
        {
            try
            {
                var connection = Classes.WorkingDB.OpenConnection();
                if (connection == null)
                {
                    MessageBox.Show("Ошибка подключения к базе данных");
                    return false;
                }

                using (var cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM users WHERE Id = @Id AND PinCode = @PinCode", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", MainWindow.mainWindow.UserLogIn.Id);
                    cmd.Parameters.AddWithValue("@PinCode", PinCodeTextBox.Text);

                    var result = cmd.ExecuteScalar();
                    Classes.WorkingDB.CloseConnection(connection);

                    return Convert.ToInt32(result) > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки PIN-кода: {ex.Message}");
                return false;
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
            if (isLoginMode)
            {
                // Возвращаемся к обычному логину
                MainWindow.mainWindow.OpenPage(new Login());
            }
            else
            {
                // Пропускаем установку PIN-кода
                MessageBox.Show("Вы сможете установить PIN-код позже. Пока используйте пароль.");
                MainWindow.mainWindow.OpenPage(new Login());
            }
        }

        private void SetPinCode_Loaded(object sender, RoutedEventArgs e)
        {
            // Фокус на поле ввода при загрузке
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                new Action(() => PinCodeTextBox.Focus()));

            // Проверяем ID пользователя (только в режиме установки)
            if (!isLoginMode && MainWindow.mainWindow.UserLogIn.Id <= 0)
            {
                MessageBox.Show("Пользователь не найден. Пожалуйста, войдите снова.");
                MainWindow.mainWindow.OpenPage(new Login());
            }
        }
    }
}