using System.Collections.Generic;
using System.Data.SqlClient;

namespace News
{
    public interface INewsDatabase
    {
        SqlConnection Connection { get; }
        string ConnectionString { get; }
        void Connect();
        void Disconnect();
        int ExecuteQuery(string query);

        List<Comment> FetchCommentsData(string query);

        SqlDataReader ExecuteSearchReader(string query, string searchedText);

        List<Post> FetchPostsData(string query, string searchedText);

        bool? ExecuteScalar(string query);
    }
}