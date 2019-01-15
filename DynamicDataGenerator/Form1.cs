using DynamicSQLConnector;
using System;
using System.Windows.Forms;

namespace DynamicDataGenerator
{
    public partial class Form1 : Form
    {
        private DataAnalyzer _dataAnalyzer;
        private KeyWords _keyWords = new KeyWords();
        private DynamicSQLConnection _connection;
        private DateTime _startTime;
        private DateTime _endTime;
        //private string _connectionString = @"Data Source=.\SQL2016;Initial Catalog=AGM_Durmont_2018;Integrated Security=True";               

        public Form1()
        {
            InitializeComponent();
            _dataAnalyzer = new DataAnalyzer(_keyWords.DeSerialize());
            _connection = new DynamicSQLConnection(_dataAnalyzer.GetConnectionStringTarget(), _dataAnalyzer.GetConnectionStringSource());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReadExcelData();
        }

        private void ReadExcelData()
        {
            
            DynamicExcelReader.DynamicExcelReader xReader = new DynamicExcelReader.DynamicExcelReader();
            string fileName = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Conversion.xlsx";
            xReader.ReadExcel(fileName, true);
            _dataAnalyzer.ObjData = xReader.ObjData;
            _dataAnalyzer.ReferenceData = xReader.ReferenceData;
            

            //MessageBox.Show(_connection.TestDynamicSQLConnection() + "\n" + "Finished: " + _dataAnalyzer.ObjData.Count.ToString() + " Tables and " +
            //    _dataAnalyzer.ReferenceData.Count.ToString() + " References.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReadKeyWordsFromFile();
        }

        private static KeyWords ReadKeyWordsFromFile()
        {
            FieldTypes fieldType = new FieldTypes("Description", 80);
            
            KeyWords keyWords = new KeyWords();
            keyWords.AddKeyWord("Name");
            keyWords.AddKeyWord("Name 2");
            keyWords.AddKeyWord("Address");
            keyWords.AddKeyWord("Address 2");
            keyWords.AddKeyWord("City");
            keyWords.AddKeyWord("Description");
            keyWords.AddFieldType(fieldType);
            keyWords.Serialize(keyWords);
            keyWords = keyWords.DeSerialize();
            return (keyWords);
        }

        private void readKeywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteReadKeyWords();
        }

        private void ExecuteReadKeyWords()
        {
            KeyWords keyWords = null;

            try
            {
                keyWords = ReadKeyWordsFromFile();
            }
            catch (Exception ex)
            {
                AddLineToRTB();
                rtbInfo.AppendText("Read KeyWords Result: " + ex.ToString());
            }

            if (keyWords != null)
            {
                AddLineToRTB();
                rtbInfo.AppendText("Read KeyWords Result (Key Words): \n");
                foreach (string k in keyWords.KeyWordList)
                {
                    rtbInfo.AppendText(k + "\n");
                }

                AddLineToRTB();
                rtbInfo.AppendText("Read KeyWords Result (Field Type List): \n");

                foreach (FieldTypes f in keyWords.FieldTypeList)
                {
                    rtbInfo.AppendText(f.FieldType + ", " + f.FieldLength.ToString() + "\n");
                }
                AddLineToRTB();
            }
            else
            {
                rtbInfo.AppendText("Read KeyWords Result (Key Words): ERROR - could not read the Keywords\n");
            }
        }

        private void AddLineToRTB()
        {
            rtbInfo.AppendText("--------------------------------------------------------------------------------\n");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void readExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteReadExcel();
        }

        private void ExecuteReadExcel()
        {
            
            AddLineToRTB();
            rtbInfo.AppendText("SQL Connection (Target) Test: ");
            rtbInfo.AppendText(_connection.TestDynamicSQLConnectionTarget() + "\n");
            AddLineToRTB();

            Cursor.Current = Cursors.WaitCursor;
            _startTime = DateTime.Now;
            ReadExcelData();
            _endTime = DateTime.Now;
            Cursor.Current = Cursors.Default;
            rtbInfo.AppendText("Finished: " + _dataAnalyzer.ObjData.Count.ToString() + " Tables");
            rtbInfo.AppendText(" and " + _dataAnalyzer.ReferenceData.Count.ToString() + " References.\n");
            rtbInfo.AppendText("Duration: " + (_endTime - _startTime).TotalSeconds.ToString() + " Seconds\n");
            AddLineToRTB();
        }

        private void updateDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecuteUpdateData();
        }

        private void ExecuteUpdateData()
        {
            AddLineToRTB();
            _startTime = DateTime.Now;
            Cursor.Current = Cursors.WaitCursor;
            _dataAnalyzer.UpdateData();
            Cursor.Current = Cursors.Default;
            _endTime = DateTime.Now;
            rtbInfo.AppendText("Finished Update Data\n");
            rtbInfo.AppendText("Duration: " + (_endTime - _startTime).TotalSeconds.ToString() + " Seconds\n");
            AddLineToRTB();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ExecuteReadKeyWords();
            ExecuteReadExcel();
            ExecuteUpdateData();
        }
    }
}
