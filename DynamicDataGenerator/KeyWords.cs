using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DynamicDataGenerator
{
    [XmlRoot("KeyWords")]
    public class KeyWords
    {
        [XmlArray("KeyWordList")]
        public List<string> KeyWordList;

        [XmlArray("FieldTypeList")]
        public List<FieldTypes> FieldTypeList;

        public KeyWords()
        {
            KeyWordList = new List<string>();
            FieldTypeList = new List<FieldTypes>();
        }

        public void AddKeyWord(string keyWord)
        {
            KeyWordList.Add(keyWord.ToUpper());
        }

        public void AddFieldType(FieldTypes fieldType)
        {
            FieldTypeList.Add(fieldType);
        }

        /// <summary>
        /// Serialization of DDG Keywords
        /// </summary>
        /// <param name="keyWords">KeyWords object holding Data for Serialization</param>
        public void Serialize(KeyWords keyWords)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(KeyWords));
            string filePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Keywords.xml";
            using (TextWriter writer = new StreamWriter(@filePath))
            {
                serializer.Serialize(writer, keyWords);
            }
        }

        /// <summary>
        /// Deserializes KeyWords object from the specified XML File
        /// </summary>
        /// <returns>KeyWords object containing DDG KeyWord data</returns>
        public KeyWords DeSerialize()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(KeyWords));
            string filePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Keywords.xml";
            TextReader reader = new StreamReader(@filePath);
            object obj = deserializer.Deserialize(reader);
            KeyWords XmlData = (KeyWords)obj;
            reader.Close();

            return ((KeyWords)XmlData);
        }
    }

    [XmlRoot("FieldTypes")]
    public class FieldTypes
    {
        private string _fieldType;

        public FieldTypes()
        {
            _fieldType = string.Empty;
            FieldLength = 0;
        }

        public FieldTypes(string fieldType, int fieldLength)
        {
            _fieldType = fieldType.ToUpper();
            FieldLength = fieldLength;
        }

        [XmlElement("FieldType")]
        public string FieldType { get => _fieldType.ToUpper(); set => _fieldType = value.ToUpper(); }

        [XmlElement("FieldLength")]
        public int FieldLength { get; set; }
    }
}
