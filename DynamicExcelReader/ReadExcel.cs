using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
        #region private members

        private string _id;
        private int _tableNo;
        private string _tableName;
        private int _fieldNo;
        private string _fieldName;
        private string _fieldType;
        private int _relationTableNo;
        private int _relationTableFieldNo;
        private string _sqlDataType;
        private bool _newValue;
        private IDictionary<int, string> _objDescription;

        #endregion

        #region Getters and Setters

        public string Id { get => _id; set => _id = value; }
        public int TableNo { get => _tableNo; set => _tableNo = value; }
        public string TableName { get => _tableName; set => _tableName = value; }
        public int FieldNo { get => _fieldNo; set => _fieldNo = value; }
        public string FieldName { get => _fieldName; set => _fieldName = value; }
        public string FieldType { get => _fieldType; set => _fieldType = value; }
        public int RelationTableNo { get => _relationTableNo; set => _relationTableNo = value; }
        public int RelationTableFieldNo { get => _relationTableFieldNo; set => _relationTableFieldNo = value; }
        public string SqlDataType { get => _sqlDataType; set => _sqlDataType = value; }
        public bool NewValue { get => _newValue; set => _newValue = value; }

        #endregion

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
                    _id = SqlFormatted(value);
                    if (_id.ToUpper() == "K")
                    {
                        _fieldName = _id;
                    }
                    break;
                case 2:
                    _tableNo = Convert.ToInt32(value);
                    break;
                case 3:
                    _tableName = SqlFormatted(value);
                    break;
                case 4:
                    _fieldNo = Convert.ToInt32(value);
                    break;
                case 5:
                    _fieldName = SqlFormatted(value);
                    break;
                case 6:
                    _fieldType = SqlFormatted(value);
                    break;
                case 7:
                    _relationTableNo = Convert.ToInt32(value);
                    break;
                case 8:
                    _relationTableFieldNo = Convert.ToInt32(value);
                    break;
                case 9:
                    _sqlDataType = SqlFormatted(value);
                    break;
                default:
                    break;
            }
        }

        private string SqlFormatted(string value)
        {
            value = value.Replace(".", "_");
            value = value.Replace("/", "_");
            return value;
        }
    }



    public class SLExcelStatus
    {
        public string Message { get; set; }
        public bool Success
        {
            get { return string.IsNullOrWhiteSpace(Message); }
        }
    }

    public class DynamicExcelReader
    {

        private IDictionary<int, List<NAVObject>> _objData = new Dictionary<int, List<NAVObject>>();
        private IDictionary<int, List<NAVObject>> _referenceData = new Dictionary<int, List<NAVObject>>();

        public IDictionary<int, List<NAVObject>> ObjData { get => _objData; set => _objData = value; }
        public IDictionary<int, List<NAVObject>> ReferenceData { get => _referenceData; set => _referenceData = value; }

        private string GetColumnName(string cellReference)
        {
            var regex = new Regex("[A-Za-z]+");
            var match = regex.Match(cellReference);

            return match.Value;
        }

        private int ConvertColumnNameToNumber(string columnName)
        {
            var alpha = new Regex("^[A-Z]+$");
            if (!alpha.IsMatch(columnName)) throw new ArgumentException();

            char[] colLetters = columnName.ToCharArray();
            Array.Reverse(colLetters);

            var convertedValue = 0;
            for (int i = 0; i < colLetters.Length; i++)
            {
                char letter = colLetters[i];
                // ASCII 'A' = 65
                int current = i == 0 ? letter - 65 : letter - 64;
                convertedValue += current * (int)Math.Pow(26, i);
            }

            return convertedValue;
        }

        private IEnumerator<Cell> GetExcelCellEnumerator(Row row)
        {
            int currentCount = 0;
            foreach (Cell cell in row.Descendants<Cell>())
            {
                string columnName = GetColumnName(cell.CellReference);

                int currentColumnIndex = ConvertColumnNameToNumber(columnName);

                for (; currentCount < currentColumnIndex; currentCount++)
                {
                    var emptycell = new Cell()
                    {
                        DataType = null,
                        CellValue = new CellValue(string.Empty)
                    };
                    yield return emptycell;
                }

                yield return cell;
                currentCount++;
            }
        }

        private string ReadExcelCell(Cell cell, WorkbookPart workbookPart)
        {
            var cellValue = cell.CellValue;
            var text = (cellValue == null) ? cell.InnerText : cellValue.Text;
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                text = workbookPart.SharedStringTablePart.SharedStringTable
                    .Elements<SharedStringItem>().ElementAt(
                        Convert.ToInt32(cell.CellValue.Text)).InnerText;
            }

            return (text ?? string.Empty).Trim();
        }

        public void ReadExcel(string fileName, bool ignoreFirstLine)
        {
            int ignoreLine = 0;
            if (ignoreFirstLine) ignoreLine = 1;

            using (SpreadsheetDocument excelDocument = SpreadsheetDocument.Open(fileName, true))
            {
                //Get workbookpart
                WorkbookPart workbookPart = excelDocument.WorkbookPart;

                //then access to the worksheet part
                IEnumerable<WorksheetPart> worksheetPart = workbookPart.WorksheetParts;

                foreach (WorksheetPart WSP in worksheetPart)
                {
                    //find sheet data
                    IEnumerable<SheetData> sheetData = WSP.Worksheet.Elements<SheetData>();

                    // Iterate through every sheet inside Excel sheet
                    foreach (SheetData SD in sheetData)
                    {
                        IEnumerable<Row> row = SD.Elements<Row>(); // Get the row IEnumerator

                        foreach (Row r in SD.Elements<Row>())
                        {
                            if (r.RowIndex > ignoreLine)
                            {
                                NAVObject navObj = CreateNAVObject(workbookPart, r);

                                if (navObj.FieldName != null)
                                {
                                    UpdateObjectData(navObj);
                                    UpdateReferences(navObj);
                                }
                            }
                        }
                    }
                }
            }
        }

        private NAVObject CreateNAVObject(WorkbookPart workbookPart, Row r)
        {
            NAVObject navObj = new NAVObject();
            int i = 1;

            foreach (Cell c in r.Elements<Cell>())
            {
                string value = ReadCellValue(c, workbookPart);
                navObj.AddObjDescription(i, value);
                i++;
            }

            return navObj;
        }

        private void UpdateObjectData(NAVObject navObj)
        {
            //* Insert complete Object into Dictionary
            if (!_objData.ContainsKey(navObj.TableNo))
            {
                List<NAVObject> newList = new List<NAVObject>();
                newList.Add(navObj);
                _objData.Add(navObj.TableNo, newList);
            }
            else
            {
                List<NAVObject> existingList = _objData[navObj.TableNo];
                existingList.Add(navObj);
            }
        }

        private void UpdateReferences(NAVObject navObj)
        {
            //* Update Reference Table
            if (navObj.RelationTableNo != 0)
            {
                if (!_referenceData.ContainsKey(navObj.RelationTableNo))
                {
                    List<NAVObject> newList = new List<NAVObject>();
                    newList.Add(navObj);
                    _referenceData.Add(navObj.RelationTableNo, newList);
                }
                else
                {
                    List<NAVObject> existingList = _referenceData[navObj.RelationTableNo];
                    existingList.Add(navObj);
                }
            }
        }

        public string ReadCellValue(Cell theCell, WorkbookPart wbPart)
        {
            string value = string.Empty;

            if (theCell != null)
            {
                value = theCell.InnerText;

                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:

                            // For shared strings, look up the value in the
                            // shared strings table.
                            var stringTable =
                                wbPart.GetPartsOfType<SharedStringTablePart>()
                                .FirstOrDefault();

                            // If the shared string table is missing, something 
                            // is wrong. Return the index that is in
                            // the cell. Otherwise, look up the correct text in 
                            // the table.
                            if (stringTable != null)
                            {
                                value =
                                    stringTable.SharedStringTable
                                    .ElementAt(int.Parse(value)).InnerText;
                            }
                            break;

                        case CellValues.Boolean:
                            switch (value)
                            {
                                case "0":
                                    value = "FALSE";
                                    break;
                                default:
                                    value = "TRUE";
                                    break;
                            }
                            break;
                    }
                }
            }

            return value;
        }

    }

    public class SLExcelData
    {
        public SLExcelStatus Status { get; set; }
        public Columns ColumnConfigurations { get; set; }
        public List<string> Headers { get; set; }
        public List<List<string>> DataRows { get; set; }
        public string SheetName { get; set; }

        public SLExcelData()
        {
            Status = new SLExcelStatus();
            Headers = new List<string>();
            DataRows = new List<List<string>>();
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
            Microsoft.Office.Interop.Excel.Workbook xlWorkbook = _xlApp.Workbooks.Open(_excelFilePath);
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
