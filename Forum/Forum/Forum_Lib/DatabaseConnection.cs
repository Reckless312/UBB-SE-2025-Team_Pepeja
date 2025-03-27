using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum_Lib
{
    internal class DatabaseConnection
    {
        public string ConnectionString { get; }
        public SqlConnection Connection { get; }

        public DatabaseConnection(string connectionString)
        {
            ConnectionString = connectionString;
            Connection = new SqlConnection(connectionString);
        }

        public void Connect()
        {
            Connection.Open();
        }

        public void Disconnect()
        {
            Connection.Close();
        }

        public DataSet ExecuteQuery(string query, string entityName)
        {
            DataSet dataSet = new DataSet();

            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, Connection);

            dataAdapter.Fill(dataSet, entityName);

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

            SqlCommand command = new SqlCommand(query , Connection);

            command.Parameters.AddWithValue("@value", value);

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
    }
}
