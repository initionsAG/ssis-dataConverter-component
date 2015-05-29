using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace DataConverter
{
    public class Events
    {
        public static void Fire(IDTSComponentMetaData100 ComponentMetaData, Type eventType, string description)
        {
            Fire(ComponentMetaData, eventType, Constants.INFO_NONE, ComponentMetaData.Name + "|" + description);
        }

        public static void Fire(IDTSComponentMetaData100 ComponentMetaData, Type eventType, int sqlType, int rowsAffected)
        {
            string description =
                rowsAffected.ToString() + " rows were affected by the " + Constants.INFO_NAME[sqlType] + " Command." +
                "|" + ComponentMetaData.Name +
                "|" + Constants.INFO_NAME[sqlType] +
                "|" + rowsAffected.ToString();

            Fire(ComponentMetaData, eventType, sqlType, description);
        }

        public static void Fire(IDTSComponentMetaData100 ComponentMetaData, Type eventType, int sqlType, string description)
        {
            bool cancel = false;

            switch (eventType)
            {
                case Type.Information:
                    ComponentMetaData.FireInformation(sqlType, ComponentMetaData.Name, description, "", 0, ref cancel);
                    break;
                case Type.Progress:
                    throw new NotImplementedException("Progress messages are not implemented");
                case Type.Warning:
                    ComponentMetaData.FireWarning(sqlType, ComponentMetaData.Name, description, string.Empty, 0);
                    break;
                case Type.Error:
                    ComponentMetaData.FireError(sqlType, ComponentMetaData.Name, description, string.Empty, 0, out cancel);
                    break;
                default:
                    ComponentMetaData.FireError(sqlType, ComponentMetaData.Name, description, string.Empty, 0, out cancel);
                    break;
            }
        }

        public enum Type
        {
            Information = 0,
            Progress = 1,
            Warning = 2,
            Error = 3
        }
    }
}
