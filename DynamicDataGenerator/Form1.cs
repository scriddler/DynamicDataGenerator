using System;
using System.Windows.Forms;
using DynamicSQLConnector;

namespace DynamicDataGenerator
{
    public partial class Form1 : Form
    {
        private DynamicSQLConnection _connection;
        private string _connectionString = @"Data Source=.\SQL2016;Initial Catalog=AGM_Durmont_2018;Integrated Security=True";
        private DataAnalyzer _dataAnalyzer = new DataAnalyzer();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _connection = new DynamicSQLConnection(_connectionString,_connectionString);
            
            

            //ReadExcel excelReader = new ReadExcel("");
            DynamicExcelReader.DynamicExcelReader xReader = new DynamicExcelReader.DynamicExcelReader();
            string fileName = @"C:\Data\Customers\Durmont\Test02.xlsx";
            xReader.ReadExcel(fileName,true);
            _dataAnalyzer.ObjData = xReader.ObjData;
            _dataAnalyzer.ReferenceData = xReader.ReferenceData;
            _dataAnalyzer.UpdateData();

            //MessageBox.Show(_connection.TestDynamicSQLConnection() + "\n" + "Finished: " + _dataAnalyzer.ObjData.Count.ToString() + " Tables and " +
            //    _dataAnalyzer.ReferenceData.Count.ToString() + " References.");

        }

    }
}
