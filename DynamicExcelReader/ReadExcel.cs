using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace DynamicExcelReader
{
    public class ExcelColumnInfo
    {
        private int _row;

        public ExcelColumnInfo(int row)
        {
            _row = row;
        }

        public int Row { get => _row; set => _row = value; }

    }

    public class NAVObject
    {
        private string _id;
        private int _tableNo;
        private string _tableName;
        private int _fieldNo;
        private string _fieldName;
        private string _fieldType;
        private IDictionary<int, string> _objDescription;

        public string Id { get => _id; set => _id = value; }
        public int TableNo { get => _tableNo; set => _tableNo = value; }
        public string TableName { get => _tableName; set => _tableName = value; }
        public int FieldNo { get => _fieldNo; set => _fieldNo = value; }
        public string FieldName { get => _fieldName; set => _fieldName = value; }
        public string FieldType { get => _fieldType; set => _fieldType = value; }

        public NAVObject()
        {
            _objDescription = new Dictionary<int, string>();
        }

        public void AddObjDescription(int column, string value)
        {
            _objDescription.Add(column, value);

            switch (column)
            {
                case 1:
                    _id = value.ToString();
                    break;
                case 2:
                    _tableNo = Convert.ToInt32(value);
                    break;
                case 3:
                    _tableName = value.ToString();
                    break;
                case 4:
                    _fieldNo = Convert.ToInt32(value);
                    break;
                case 5:
                    _fieldName = value;
                    break;
                case 6:
                    _fieldType = value;
                    break;
                default:
                    break;
            }
        }
    }


    public class ReadExcel
    {
        private readonly string _excelFilePath;
        private Application _xlApp = new Application();
        private IDictionary<string, NAVObject> _objData = new Dictionary<string, NAVObject>();

        public ReadExcel(string excelFilePath)
        {
            excelFilePath = @"C:\Data\Customers\Durmont\Test01.xlsx";
            _excelFilePath = @excelFilePath;
            OpenExcelSheet();
        }

        private int OpenExcelSheet()
        {
            Workbook xlWorkbook = _xlApp.Workbooks.Open(_excelFilePath);
            _Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Range xlRange = xlWorksheet.UsedRange;

            int rowCount = xlWorksheet.Rows.Count;
            rowCount = 100; // TEST
            int colCount = 6; // xlWorksheet.Columns.Count;

            //iterate over the rows and columns and print to the console as it appears in the file
            //excel is not zero based!!
            for (int i = 1; i <= rowCount; i++)
            {
                if (i > 1)
                {
                    NAVObject navObj = new NAVObject();

                    for (int j = 1; j <= colCount; j++)
                    {
                        ////new line
                        //if (j == 1)
                        //    Console.Write("\r\n");

                        //write the value to the console
                        if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                            navObj.AddObjDescription(j, xlRange.Cells[i, j].Value2.ToString());
                        //_excelData.Add(new ExcelColumnInfo(i, j), xlRange.Cells[i, j].Value2.ToString());
                        //Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");

                        if (j == colCount)
                        {
                            if ((navObj.FieldName.ToUpper().Contains("NAME")) || (navObj.FieldName.ToUpper().Contains("ADDRESS")))
                            {
                                if ((navObj.FieldType.ToUpper() == "CODE") || (navObj.FieldType.ToUpper() == "TEXT"))
                                {
                                    _objData.Add(navObj.TableNo.ToString() + "_" + navObj.FieldName, navObj);
                                }
                            }
                        }

                    }
                }
            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            _xlApp.Quit();
            Marshal.ReleaseComObject(_xlApp);


            return (_objData.Count);

        }



    }
}
