
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Regin_New.Pages
{
    public partial class SetPinCode : Page
    {
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
            if (PinCodeTextBox.Text.Length == 4)
            {
                MainWindow.mainWindow.UserLogIn.PinCode = PinCodeTextBox.Text;
                SavePinCodeToDatabase();

                MessageBox.Show("Пин-код успешно установлен! Теперь вы можете использовать его для быстрой авторизации.");
                MainWindow.mainWindow.OpenPage(new Login());
            }
            else if (PinCodeTextBox.Text.Length > 4)
            {
                PinCodeTextBox.Text = PinCodeTextBox.Text.Substring(0, 4);
                PinCodeTextBox.CaretIndex = 4;
            }
        }

        private void SavePinCodeToDatabase()
        {
            var connection = Classes.WorkingDB.OpenConnection();
            if (connection != null)
            {
                try
                {
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                        "UPDATE users SET PinCode = @PinCode WHERE Id = @Id", connection))
                    {
                        cmd.Parameters.AddWithValue("@PinCode", MainWindow.mainWindow.UserLogIn.PinCode);
                        cmd.Parameters.AddWithValue("@Id", MainWindow.mainWindow.UserLogIn.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    Classes.WorkingDB.CloseConnection(connection);
                }
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы можете установить пин-код позже в настройках профиля.");
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}