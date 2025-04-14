using System.Data.SqlClient;

namespace SteamCommunity.Reviews.Database
{
    public interface IDatabaseConnection
    {
        SqlConnection GetConnection();
        void Connect(SqlConnection connection);
        void Disconnect(SqlConnection connection);
        SqlCommand ExecuteQuery(string query, SqlConnection connection);
    }
}