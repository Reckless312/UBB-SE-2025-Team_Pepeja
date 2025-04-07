using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Search
{
    public class DatabaseConnection
    {
        private const string CONNECTION_STRING = "Integrated Security=True;TrustServerCertificate=True;data source=DESKTOP-BI53R5C\\SQLEXPRESS02;initial catalog=Steam;user id=sa";
        public string ConnectionString { get; }
        public SqlConnection Connection { get; }

        public DatabaseConnection()
        {
            ConnectionString = CONNECTION_STRING;
            Connection = new SqlConnection(CONNECTION_STRING);
        }

        public void Connect()
        {
            Connection.Open();
        }

        public void Disconnect()
        {
            Connection.Close();
        }

        public DataSet ExecuteQuery(string query, string tableName)
        {
            DataSet dataSet = new DataSet();

            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, Connection);

            dataAdapter.Fill(dataSet, tableName);

            return dataSet;
        }

        public void ExecuteInsert(string tableName, Dictionary<string, object> parameters)
        {
            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (var param in parameters)
            {
                columns.Append(param.Key + ", ");
                values.Append("@" + param.Key + ", ");
            }

            columns.Length -= 2;
            values.Length -= 2;

            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

            SqlCommand command = new SqlCommand(query, Connection);

            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue("@" + param.Key, param.Value);
            }

            command.ExecuteNonQuery();
        }

        public void ExecuteDelete(string tableName, string columnName, object value)
        {
            string query = $"DELETE FROM {tableName} WHERE {columnName} = @Value";

            SqlCommand command = new SqlCommand(query, Connection);

            command.Parameters.AddWithValue("@value", value);

            command.ExecuteNonQuery();
        }

        public void ExecuteDeleteWithAnd(string tableName, Dictionary<string, object> parameters)
        {
            StringBuilder query = new StringBuilder();

            if (parameters.Count == 0)
            {
                return;
            }

            query.Append($"DELETE FROM {tableName} WHERE");

            foreach (var param in parameters)
            {
                query.Append($" {param.Key} = @{param.Key} AND");
            }

            int numberOfCharactersToBeRemoved = 3;
            query.Remove(query.Length - numberOfCharactersToBeRemoved, numberOfCharactersToBeRemoved);

            SqlCommand command = new SqlCommand(query.ToString(), Connection);

            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue("@" + param.Key, param.Value);
            }

            command.ExecuteNonQuery();
        }

        public void ExecuteUpdate(string tableName, string columnToUpdate, string columnToMatch, object updateValue, object matchValue)
        {
            string query = $"UPDATE {tableName} SET {columnToUpdate} = @columnToUpdate WHERE {columnToMatch} = @matchValue";

            SqlCommand command = new SqlCommand(query, Connection);

            command.Parameters.AddWithValue("@columnToUpdate", updateValue);
            command.Parameters.AddWithValue("@matchValue", matchValue);

            command.ExecuteNonQuery();
        }

        public void ExecuteNonQuery(string query)
        {
            SqlCommand command = new SqlCommand(query, Connection);
            command.ExecuteNonQuery();
        }
    }
}