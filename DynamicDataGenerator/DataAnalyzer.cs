using DynamicExcelReader;
using DynamicSQLConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DynamicDataGenerator
{
    public class DataAnalyzer
    {
        private List<string> _topicList = new List<string>();
        private IDictionary<int, List<NAVObject>> _objData = new Dictionary<int, List<NAVObject>>();
        private IDictionary<int, List<NAVObject>> _referenceData = new Dictionary<int, List<NAVObject>>();
        private IDictionary<Status, NAVObject> _conversionList = new Dictionary<Status, NAVObject>();
        private IDictionary<string, List<string>> _conversionValues = new Dictionary<string, List<string>>();
        private IDictionary<string, List<string>> _targetValues = new Dictionary<string, List<string>>();

        public enum Status
        {
            Success,
            Failure
        }

        public DataAnalyzer()
        {
            FillTopicList();
        }

        public List<string> TopicList { get => _topicList; set => _topicList = value; }
        public IDictionary<int, List<NAVObject>> ObjData { get => _objData; set => _objData = value; }
        public IDictionary<int, List<NAVObject>> ReferenceData { get => _referenceData; set => _referenceData = value; }
        public IDictionary<Status, NAVObject> ConversionList { get => _conversionList; set => _conversionList = value; }

        public void UpdateData()
        {
            ParseDictionaryData();

            //* Update Object Data           
            foreach (var entry in _objData)
            {
                foreach(NAVObject navObj in entry.Value)
                {
                    if (navObj.NewValue)
                    {
                        string key = string.Format("{0}_{1}", navObj.TableNo.ToString(), navObj.FieldNo.ToString());
                        UpdateTargetData(navObj, key);
                    }
                }
            }
        }

        private void ParseDictionaryData()
        {
            DynamicSQLConnection dynSQLConnection = null;

            foreach (var entry in _objData)
            {
                foreach (NAVObject navObj in entry.Value)
                {
                    if (NavObjectContainsKeyWord(navObj))
                    {
                        string key = string.Format("{0}_{1}", navObj.TableNo.ToString(), navObj.FieldNo.ToString());

                        if (_conversionValues.ContainsKey(key))
                        {
                            navObj.NewValue = true;
                        }
                        else
                        {
                            if (navObj.Id != "K") // do not Parse Table Keys
                            {
                                dynSQLConnection = ParseDictionaryTable(dynSQLConnection, navObj, key);
                            }
                        }
                    }
                }
            }
        }

        private DynamicSQLConnection ParseDictionaryTable(DynamicSQLConnection dynSQLConnection, NAVObject navObj, string key)
        {
            try
            {
                dynSQLConnection = new DynamicSQLConnection(GetConnectionStringTarget(), GetConnectionStringSource());
                SqlCommand SQLCmd = new SqlCommand();
                // TODO: replace more than that..
                SQLCmd.CommandText = string.Format("SELECT [{0}] FROM[DDC_Dictionary].[dbo].[CRONUS AG${1}]", navObj.FieldName, navObj.TableName);
                SQLCmd.CommandType = CommandType.Text;
                SQLCmd.Connection = dynSQLConnection.SqlConnectionSource;
                SQLCmd.CommandTimeout = 0;
                dynSQLConnection.SqlConnectionSource.Open();
                SqlDataReader sqlReader = SQLCmd.ExecuteReader();

                if (sqlReader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(sqlReader);
                    List<string> result = new List<string>();

                    foreach (var r in dt.AsEnumerable())
                    {
                        result.Add(r.ItemArray[0].ToString());
                    }

                    if (result != null)
                    {
                        _conversionValues.Add(key, result);
                        navObj.NewValue = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                try
                {
                    dynSQLConnection.CloseAllConnections();
                }
                catch (Exception)
                {
                }
            }

            return dynSQLConnection;
        }

        private void UpdateTargetData(NAVObject navObj, string key)
        {
            NAVObject keyNavObject = GetKeyNavObject(navObj.TableNo);

            if (keyNavObject != null)
            {
                //TODO: Parse Key String (can have ",")
                DynamicSQLConnection dynSQLConnection = null;

                try
                {
                    dynSQLConnection = new DynamicSQLConnection(GetConnectionStringTarget(), GetConnectionStringSource());
                    SqlCommand SQLCmd = new SqlCommand();
                    // TODO: replace more than that..
                    SQLCmd.CommandText = string.Format("SELECT [{1}], [{0}] FROM[AGM_Durmont_2018].[dbo].[CRONUS AG${2}]", navObj.FieldName, keyNavObject.FieldName, navObj.TableName);
                    SQLCmd.CommandType = CommandType.Text;
                    SQLCmd.Connection = dynSQLConnection.SqlConnectionTarget;
                    SQLCmd.CommandTimeout = 0;
                    dynSQLConnection.SqlConnectionSource.Open();
                    SqlDataReader sqlReader = SQLCmd.ExecuteReader();

                    if (sqlReader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(sqlReader);
                        List<string> result = new List<string>();

                        foreach (var r in dt.AsEnumerable())
                        {
                            result.Add(r.ItemArray[0].ToString());
                            // TODO: UPDATE Query HERE
                        }

                        if (result != null)
                        {
                            //_conversionValues.Add(key, result);
                            //navObj.NewValue = true;
                        }
                    }


                    //dynSQLConnection = new DynamicSQLConnection(GetConnectionStringTarget(), GetConnectionStringSource());
                    //SqlCommand SQLCmd = new SqlCommand();
                    //// TODO: replace more than that..                
                    //SQLCmd.CommandText = string.Format("UPDATE [AGM_Durmont_2018].[dbo].[CRONUS AG${1}] SET [{0}] = '{2}'", navObj.FieldName, navObj.TableName, GetRandomValue(key));
                    //SQLCmd.CommandType = CommandType.Text;
                    //SQLCmd.Connection = dynSQLConnection.SqlConnectionTarget;
                    //SQLCmd.CommandTimeout = 0;
                    //dynSQLConnection.SqlConnectionSource.Open();
                    //SqlDataReader sqlReader = SQLCmd.ExecuteReader();

                    //if (sqlReader.HasRows)
                    //{
                    //    DataTable dt = new DataTable();
                    //    dt.Load(sqlReader);
                    //    List<string> result = new List<string>();

                    //    foreach (var r in dt.AsEnumerable())
                    //    {
                    //        result.Add(r.ItemArray[0].ToString());
                    //    }

                    //    if (result != null)
                    //    {
                    //        _conversionValues.Add(key, result);
                    //        navObj.NewValue = true;
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                finally
                {
                    try
                    {
                        dynSQLConnection.CloseAllConnections();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private NAVObject GetKeyNavObject(int tableNo)
        {
            NAVObject keyDefinition = null;
            List<NAVObject> navObjects = _objData[tableNo];

            foreach (NAVObject navObj in navObjects)
            {
                if (navObj.Id.ToUpper() == "K")
                {
                    keyDefinition = navObj;
                    break;
                }
            }
            return keyDefinition;
        }

        private string GetRandomValue(string key)
        {
            List<string> newValues = _conversionValues[key];
            Random rnd = new Random(newValues.Count);

            return newValues[rnd.Next(newValues.Count)];
        }

        private string GetConnectionStringTarget()
        {
            // To avoid storing the connection string in your code, 
            // you can retrieve it from a configuration file, using the 
            // System.Configuration.ConfigurationSettings.AppSettings property 
            return @"Data Source=.\SQL2016;Initial Catalog=AGM_Durmont_2018;Integrated Security=True";
        }

        private string GetConnectionStringSource()
        {
            // To avoid storing the connection string in your code, 
            // you can retrieve it from a configuration file, using the 
            // System.Configuration.ConfigurationSettings.AppSettings property 
            return @"Data Source=.\SQL2016;Initial Catalog=DDC_Dictionary;Integrated Security=True";
        }


        private void FillTopicList()
        {
            // TODO: Read from somewhere else...
            _topicList.Add("NAME");
            _topicList.Add("ADDRESS");
            _topicList.Add("DESCRIPTION");
            _topicList.Add("CITY");
        }

        private bool NavObjectContainsKeyWord(NAVObject navObj)
        {
            bool found = false;
            foreach (string k in _topicList)
            {
                if (navObj.FieldName.ToUpper().Contains(k))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }
}
