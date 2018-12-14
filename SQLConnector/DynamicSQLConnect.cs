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
        private SqlConnection _sqlConnection;

        public DynamicSQLConnection(string connectionString)
        {
            _sqlConnection = new SqlConnection(connectionString);
        }

        public string TestDynamicSQLConnection()
        {
            _sqlConnection.Open();
            string test = _sqlConnection.Database.ToString();
            _sqlConnection.Close();
            return test;
        }
        static private string GetConnectionString()
        {
            // To avoid storing the connection string in your code, 
            // you can retrieve it from a configuration file, using the 
            // System.Configuration.ConfigurationSettings.AppSettings property 
            return @"Data Source=.\SQL2016;Initial Catalog=AGM_Durmont_2018;Integrated Security=True";
        }



    }
}
