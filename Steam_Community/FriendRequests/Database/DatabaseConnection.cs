using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace App1.Database
{
    public class DatabaseConnection
    {
        private readonly string _connectionString = "Data Source=DESKTOP-2OA983C;Initial Catalog=Community;Integrated Security=True;TrustServerCertificate=True;";

        // Create a new connection each time rather than reusing a single connection
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<T> ExecuteScalarAsync<T>(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return (T)result;
        }

        public async Task ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            var dataTable = new DataTable();
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);
            
            return dataTable;
        }

        // For backward compatibility
        public void ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();
            command.ExecuteNonQuery();
        }

        public T ExecuteScalar<T>(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();
            return (T)command.ExecuteScalar();
        }

        public DataTable ExecuteReader(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            var dataTable = new DataTable();
            connection.Open();
            using var reader = command.ExecuteReader();
            dataTable.Load(reader);
            
            return dataTable;
        }
    }
} 