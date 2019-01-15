using System.Data.SqlClient;

namespace DynamicSQLConnector
{
    public class DynamicSQLConnection
    {
        public DynamicSQLConnection(string connectionStringTarget, string connectionStringSource)
        {
            SqlConnectionTarget = new SqlConnection(connectionStringTarget);
            SqlConnectionSource = new SqlConnection(connectionStringSource);
        }

        public SqlConnection SqlConnectionSource { get; set; }
        public SqlConnection SqlConnectionTarget { get; set; }

        public string TestDynamicSQLConnectionTarget()
        {
            SqlConnectionTarget.Open();
            string test = SqlConnectionTarget.Database.ToString();
            SqlConnectionTarget.Close();
            return test;
        }

        public void CloseAllConnections()
        {
            SqlConnectionTarget.Close();
            SqlConnectionTarget.Dispose();
            SqlConnectionSource.Close();
            SqlConnectionSource.Dispose();
        }
        
    }
}
