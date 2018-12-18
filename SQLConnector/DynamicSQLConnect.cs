using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSQLConnector
{
    public class DynamicSQLConnection
    {
        private SqlConnection _sqlConnectionSource;
        private SqlConnection _sqlConnectionTarget;

        public DynamicSQLConnection(string connectionStringTarget, string connectionStringSource)
        {
            _sqlConnectionTarget = new SqlConnection(connectionStringTarget);
            _sqlConnectionSource = new SqlConnection(connectionStringSource);
        }

        public SqlConnection SqlConnectionSource { get => _sqlConnectionSource; set => _sqlConnectionSource = value; }
        public SqlConnection SqlConnectionTarget { get => _sqlConnectionTarget; set => _sqlConnectionTarget = value; }

        public string TestDynamicSQLConnectionTarget()
        {
            _sqlConnectionTarget.Open();
            string test = _sqlConnectionTarget.Database.ToString();
            _sqlConnectionTarget.Close();
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
