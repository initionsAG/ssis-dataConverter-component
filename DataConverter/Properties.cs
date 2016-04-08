using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Reflection;
using System.Diagnostics;

namespace DataConverter
{
    public class CustomProperties
    {
        public string version
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                return fvi.FileVersion;
            }
        }

        public string propVersion
        {
            get { return ComponentMetaData.CustomPropertyCollection[Constants.PROP_VERSION].Value.ToString(); }
            set { ComponentMetaData.CustomPropertyCollection[Constants.PROP_VERSION].Value = value; }
        }

        private IDTSComponentMetaData100 _componentMetaData;
        public IDTSComponentMetaData100 ComponentMetaData
        {
            get { return _componentMetaData; }
            set { _componentMetaData = value; }
        }

        public string PrefixOutput
        {
            get { return ComponentMetaData.CustomPropertyCollection[Constants.PROP_PREFIX_OUTPUT_COL_NAME].Value.ToString(); }
            set { ComponentMetaData.CustomPropertyCollection[Constants.PROP_PREFIX_OUTPUT_COL_NAME].Value = value; }
        }

        public object[] GetConfigRow(int index)
        {
            return (object[])ComponentMetaData.CustomPropertyCollection[Constants.PROP_OUTPUT_CONFIGURATION + index.ToString()].Value;
        }

        public int MappingRowCount
        {
            get { return (int)ComponentMetaData.CustomPropertyCollection[Constants.PROP_OUTPUT_CONFIGURATION_COUNT].Value; }
            set { ComponentMetaData.CustomPropertyCollection[Constants.PROP_OUTPUT_CONFIGURATION_COUNT].Value = value; }
        }

        public void RemovePropertyByName(string name)
        {
            int ID = (int)ComponentMetaData.CustomPropertyCollection[name].ID;
            ComponentMetaData.CustomPropertyCollection.RemoveObjectByID(ID);
        }

        public void RemoveMappings()
        {
            for (int i = 0; i < MappingRowCount; i++)
            {
                RemovePropertyByName(Constants.PROP_OUTPUT_CONFIGURATION + i.ToString());
            }

            ComponentMetaData.CustomPropertyCollection[Constants.PROP_OUTPUT_CONFIGURATION_COUNT].Value = 0;
        }

        public void AddProperty(int idx, string name, object value)
        {
            IDTSCustomProperty100 prop = ComponentMetaData.CustomPropertyCollection.New();
            prop.Name = name + idx.ToString();
            prop.Value = value;

            prop = ComponentMetaData.CustomPropertyCollection[Constants.PROP_OUTPUT_CONFIGURATION_COUNT];
            prop.Value = (int)prop.Value + 1;
        }

        public bool isUsed(string inputColumnName) {
            bool result = false;

            for (int i = 0; i < MappingRowCount; i++)
			{
			   if (GetConfigRow(i)[Constants.MAPPING_IDX_INPUT_COL_NAME].ToString() == inputColumnName) 
               {
                   result = (bool)GetConfigRow(i)[Constants.MAPPING_IDX_USE];
               }
			}
        
            return result;
        }

        public object[] GetConfigRow(string inputColumnName)
        {
            object[] result = null;

            for (int i = 0; i < MappingRowCount; i++)
            {
                if (GetConfigRow(i)[Constants.MAPPING_IDX_INPUT_COL_NAME].ToString() == inputColumnName)
                {
                    result = GetConfigRow(i);
                }
            }

            return result;
        }

        public object[] GetMappingRow(int index)
        {
            return (object[])ComponentMetaData.CustomPropertyCollection[Constants.PROP_OUTPUT_CONFIGURATION + index.ToString()].Value;
        }

        public void SetMappingRow(int index, object[] row)
        {
            ComponentMetaData.CustomPropertyCollection[Constants.PROP_OUTPUT_CONFIGURATION + index.ToString()].Value = row;
        }
    }
}
