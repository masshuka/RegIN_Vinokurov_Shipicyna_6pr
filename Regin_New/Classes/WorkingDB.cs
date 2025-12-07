using MySql.Data.MySqlClient;
using System.Diagnostics;
using System;

namespace Regin_New.Classes
{
    public static class WorkingDB
    {
        private const string ConnectionString = "server=localhost; port=3306; database=regin; user=root; pwd=;";

        public static MySqlConnection OpenConnection()
        {
            try
            {
                var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                return connection;
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
                return null;
            }
        }

        public static MySqlDataReader Query(string Sql, MySqlConnection connection) =>
            new MySqlCommand(Sql, connection).ExecuteReader();

        public static void CloseConnection(MySqlConnection connection)
        {
            if (connection?.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                MySqlConnection.ClearPool(connection);
            }
        }

        public static bool OpenConnection(MySqlConnection connection) =>
            connection != null && connection.State == System.Data.ConnectionState.Open;
    }
}
