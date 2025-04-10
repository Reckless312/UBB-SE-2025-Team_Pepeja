using System.Data.SqlClient;

namespace News
{
    public interface INewsDatabase
    {
        void Connect(SqlConnection connection);
        void Disconnect(SqlConnection connection);
        SqlCommand ExecuteQuery(string query, SqlConnection connection);
        SqlDataReader ExecuteReader(string query, SqlConnection connection);
        SqlConnection GetConnection();
    }
}