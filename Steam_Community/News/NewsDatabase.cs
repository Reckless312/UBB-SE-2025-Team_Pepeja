using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News
{
    public class NewsDatabase : INewsDatabase
    {
        private readonly string connectionString;

        public NewsDatabase()
        {
            connectionString = "Data Source=DESKTOP-FL15V3S\\SQLEXPRESS; Initial Catalog=Community; Integrated Security=True; TrustServerCertificate=True;";
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

        public SqlDataReader ExecuteReader(string query, SqlConnection connection)
        {
            try
            {
                SqlCommand command = new SqlCommand(query, connection);
                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error executing reader: {ex.Message}");
                throw;
            }
        }
    }
}
