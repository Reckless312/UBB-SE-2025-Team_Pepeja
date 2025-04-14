using System.Data.SqlClient;
using System;

namespace SteamCommunity.Reviews.Database
{
    public class DatabaseConnection
    {
        private readonly string connectionString;

        public DatabaseConnection()
        {
            connectionString = "Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-BI53R5C\\SQLEXPRESS02;initial catalog=Steam;user id=sa";
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public void Connect(SqlConnection connection)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework or simply write to the console)
                Console.WriteLine($"Error connecting to the database: {ex.Message}");
                throw;
            }
        }

        public void Disconnect(SqlConnection connection)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework or simply write to the console)
                Console.WriteLine($"Error disconnecting from the database: {ex.Message}");
                throw;
            }
        }

        public SqlCommand ExecuteQuery(string query, SqlConnection connection)
        {
            try
            {
                return new SqlCommand(query, connection);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework or simply write to the console)
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }
        }



    }
}