using System;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using System.Reflection;
using System.Diagnostics;

namespace DataConverter.ComponentFrameWork.Mapping
{
    public class Mapping
    {
        public static string IdPropertyName = "CustomID";

        public static bool NeedsMapping()
        {
#if     (SQL2008) 
            return false;      
#else
            return true;
#endif
        }

        #region Output

        private static void UpdateOutputInColumnConfig(ColumnConfig config, IDTSOutputColumn100 col, string outputName, IDTSComponentMetaData100 _componentMetaData, IsagCustomProperties _isagCustomProperties)
        {

            if (outputName == Constants.OUTPUT_LOG_NAME)
            {
                config.OutputErrorId = col.ID.ToString();
                config.OutputErrorIdString = col.IdentificationString;
                config.OutputErrorLineageId = col.LineageID.ToString();
            }
            else if (outputName == Constants.OUTPUT_NAME)
            {
                config.OutputId = col.ID.ToString();
                config.OutputIdString = col.IdentificationString;
                config.OutputLineageId = col.LineageID.ToString();
            }

            _isagCustomProperties.Save(_componentMetaData);
        }

        public static void AddOutputIdProperties(IDTSOutput100 output, IDTSComponentMetaData100 _componentMetaData, IsagCustomProperties _isagCustomProperties)
        {
            if (!NeedsMapping()) return;

            foreach (IDTSOutputColumn100 col in output.OutputColumnCollection)
            {
                AddOutputIdProperties(col, output.Name, _componentMetaData, _isagCustomProperties);
            }
        }

        public static void AddOutputIdProperties(IDTSOutputColumn100 col, string outputName, IDTSComponentMetaData100 _componentMetaData, IsagCustomProperties _isagCustomProperties)
        {
            if (!NeedsMapping()) return;

            try
            {
                IDTSCustomProperty100 prop = col.CustomPropertyCollection[IdPropertyName];
                string guid = (string)prop.Value;


                foreach (ColumnConfig config in _isagCustomProperties.ColumnConfigList)
                {
                    if (config.CustomId == guid) UpdateOutputInColumnConfig(config, col, outputName, _componentMetaData, _isagCustomProperties);
                }

            }
            catch (Exception)
            {
                AddIdProperty(Guid.NewGuid().ToString() + "fehler", col.CustomPropertyCollection);
                AddOutputIdProperties(col, outputName, _componentMetaData, _isagCustomProperties);
            }


        }

        #endregion

        #region Input

        public static void UpdateInputIdProperties(IDTSComponentMetaData100 _componentMetaData, IsagCustomProperties _isagCustomProperties)
        {
            if (!NeedsMapping()) return;

            UpdateInputIdProperties(_componentMetaData.InputCollection[0], _componentMetaData, _isagCustomProperties);
        }

        private static void UpdateInputIdProperties(IDTSInput100 input, IDTSComponentMetaData100 _componentMetaData, IsagCustomProperties _isagCustomProperties)
        {
            foreach (IDTSInputColumn100 col in input.InputColumnCollection)
            {
                UpdateInputIdProperty(col, _componentMetaData, _isagCustomProperties);
            }
        }

        private static void UpdateInputIdProperty(IDTSInputColumn100 col, IDTSComponentMetaData100 _componentMetaData, IsagCustomProperties _isagCustomProperties)
        {
            try
            {
                IDTSCustomProperty100 prop = col.CustomPropertyCollection[IdPropertyName];
                string guid = (string)col.CustomPropertyCollection[IdPropertyName].Value;

                foreach (ColumnConfig config in _isagCustomProperties.ColumnConfigList)
                {
                    if (AreIdsEqual(config, guid)) UpdateColumnConfig(config, col, _componentMetaData, _isagCustomProperties);
                    //{
                    //    if ((int)prop.Value != col.ID)
                    //    {
                    //        UpdateColumnConfig(config, col);
                    //        //prop.Value = col.ID;
                    //    }
                    //}
                }
            }
            catch (Exception)
            {
                AddIdProperty(Guid.NewGuid().ToString(), col.CustomPropertyCollection);
                UpdateInputIdProperty(col, _componentMetaData, _isagCustomProperties);
            }

        }

        private static bool AreIdsEqual(ColumnConfig config, string guid)
        {
            return (config.CustomId == guid.ToString());
        }

        private static void UpdateColumnConfig(ColumnConfig config, IDTSInputColumn100 col, IDTSComponentMetaData100 _componentMetaData, IsagCustomProperties _isagCustomProperties)
        {
            config.InputId = col.ID.ToString();
            config.InputIdString = col.IdentificationString;
            config.InputLineageId = col.LineageID.ToString();

            _isagCustomProperties.Save(_componentMetaData);
        }

        #endregion

        private static void AddIdProperty(string ID, IDTSCustomPropertyCollection100 propCollection)
        {
            IDTSCustomProperty100 prop = propCollection.New();
            prop.Name = IdPropertyName;
            prop.Value = ID;
        }

        public static void SetIdProperty(string ID, IDTSCustomPropertyCollection100 propCollection)
        {
            if (!NeedsMapping()) return;
            
            try
            {
                propCollection[IdPropertyName].Value = ID;
            }
            catch 
            {
                AddIdProperty(ID, propCollection);
            }
        }

        public static bool HasIdProperty(IDTSCustomPropertyCollection100 propCollection)
        {
            if (!NeedsMapping()) return false;

            try
            {
                object value = propCollection[IdPropertyName].Value;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
