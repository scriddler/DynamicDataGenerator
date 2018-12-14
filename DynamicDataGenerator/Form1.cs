using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DynamicSQLConnector;
using DynamicExcelReader;

namespace DynamicDataGenerator
{
    public partial class Form1 : Form
    {
        private DynamicSQLConnector.DynamicSQLConnection _connection;
        private string _connectionString = @"Data Source=.\SQL2016;Initial Catalog=AGM_Durmont_2018;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _connection = new DynamicSQLConnection(_connectionString);
            
            MessageBox.Show(_connection.TestDynamicSQLConnection());

            ReadExcel excelReader = new ReadExcel("");
        }
    }
}
