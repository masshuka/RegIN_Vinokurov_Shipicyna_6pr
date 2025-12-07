using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Regin_New.Classes
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public byte[] Image = new byte[0];
        public DateTime DateUpdate { get; set; }
        public DateTime DateCreate { get; set; }
        public CorrectLogin HandlerCorrectLogin;
        public InCorrectLogin HandlerInCorrectLogin;

        public delegate void CorrectLogin();
        public delegate void InCorrectLogin();

        public void GetUserLogin(string Login)
        {
            ResetUserData();

            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();

            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                MySqlDataReader userQuery = WorkingDB.Query($"SELECT * FROM users WHERE Login = '{Login}'", mySqlConnection);

                if (userQuery.HasRows)
                {
                    userQuery.Read();
                    LoadUserData(userQuery);
                    HandlerCorrectLogin?.Invoke();
                }
                else
                {
                    HandlerInCorrectLogin?.Invoke();
                }
            }
            else
            {
                HandlerInCorrectLogin?.Invoke();
            }

            WorkingDB.CloseConnection(mySqlConnection);
        }

        private void ResetUserData()
        {
            Id = -1;
            Login = String.Empty;
            Password = String.Empty;
            Name = String.Empty;
            Image = new byte[0];
        }

        private void LoadUserData(MySqlDataReader reader)
        {
            Id = reader.GetInt32(0);
            Login = reader.GetString(1);
            Password = reader.GetString(2);
            Name = reader.GetString(3);

            if (!reader.IsDBNull(4))
            {
                LoadImageData(reader);
            }

            DateUpdate = reader.GetDateTime(5);
            DateCreate = reader.GetDateTime(6);
        }

        private void LoadImageData(MySqlDataReader reader)
        {
            try
            {
                long dataSize = reader.GetBytes(4, 0, null, 0, 0);

                if (dataSize > 0)
                {
                    byte[] imageData = new byte[dataSize];
                    reader.GetBytes(4, 0, imageData, 0, (int)dataSize);
                    Image = imageData;
                }
            }
            catch (Exception ex)
            {
                Image = new byte[0];
                Debug.WriteLine($"Ошибка чтения изображения: {ex.Message}");
            }
        }

        public void SetUser()
        {
            ExecuteDatabaseCommand(
                "INSERT INTO users (Login, Password, Name, Image, DateUpdate, DateCreate) " +
                "VALUES (@Login, @Password, @Name, @Image, @DateUpdate, @DateCreate)",
                cmd => AddUserParameters(cmd),
                cmd => cmd.ExecuteNonQuery()
            );
        }

        public void CrateNewPassword()
        {
            if (String.IsNullOrEmpty(Login)) return;

            Password = GeneratePass();

            ExecuteDatabaseCommand(
                $"UPDATE users SET Password=@Password WHERE Login = @Login",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Password", Password);
                    cmd.Parameters.AddWithValue("@Login", Login);
                },
                cmd => cmd.ExecuteNonQuery()
            );

            SendMail.SendMessage($"Your account password has been changed.\nNew password: {Password}", Login);
        }

        private void ExecuteDatabaseCommand(string query, Action<MySqlCommand> addParameters, Action<MySqlCommand> executeAction)
        {
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();

            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                {
                    addParameters(mySqlCommand);
                    executeAction(mySqlCommand);
                }
            }

            WorkingDB.CloseConnection(mySqlConnection);
        }

        private void AddUserParameters(MySqlCommand command)
        {
            command.Parameters.AddWithValue("@Login", Login);
            command.Parameters.AddWithValue("@Password", Password);
            command.Parameters.AddWithValue("@Name", Name);
            command.Parameters.AddWithValue("@Image", Image);
            command.Parameters.AddWithValue("@DateUpdate", DateUpdate);
            command.Parameters.AddWithValue("@DateCreate", DateCreate);
        }

        public string GeneratePass()
        {
            List<Char> NewPassword = new List<char>();
            Random rnd = new Random();

            char[] ArrNumbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] ArrSymbols = { '-', '_', '!', '@', '#', '$', '%', '^', '&', '*' };
            char[] ArrUppercase = { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };

            // Добавляем обязательные символы
            NewPassword.Add(ArrNumbers[rnd.Next(0, ArrNumbers.Length)]);
            NewPassword.Add(ArrSymbols[rnd.Next(0, ArrSymbols.Length)]);

            for (int i = 0; i < 2; i++)
            {
                NewPassword.Add(char.ToUpper(ArrUppercase[rnd.Next(0, ArrUppercase.Length)]));
            }

            for (int i = 0; i < 6; i++)
            {
                NewPassword.Add(ArrUppercase[rnd.Next(0, ArrUppercase.Length)]);
            }

            // Перемешиваем символы (ваш оригинальный алгоритм)
            for (int i = 0; i < NewPassword.Count; i++)
            {
                int RandomSymbol = rnd.Next(0, NewPassword.Count);
                char Symbol = NewPassword[RandomSymbol];
                NewPassword[RandomSymbol] = NewPassword[i];
                NewPassword[i] = Symbol;
            }

            // Собираем пароль в строку
            return new string(NewPassword.ToArray());
        }
    }
}
