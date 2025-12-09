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
                string newPinCode = PinCodeTextBox.Text;
                // Обновляем PIN-код в объекте пользователя и в базе данных
                MainWindow.mainWindow.UserLogIn.UpdatePinCode(newPinCode);

                MessageBox.Show("Пин-код успешно установлен!");
                MainWindow.mainWindow.OpenPage(new Login());
            }
            else if (PinCodeTextBox.Text.Length > 4)
            {
                PinCodeTextBox.Text = PinCodeTextBox.Text.Substring(0, 4);
                PinCodeTextBox.CaretIndex = 4;
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы можете установить пин-код позже в настройках профиля.");
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}