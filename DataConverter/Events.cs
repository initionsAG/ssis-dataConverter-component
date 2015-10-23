using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

namespace DataConverter
{
    public class Events
    {
        /// <summary>
        /// Fires an SSIS event
        /// </summary>
        /// <param name="ComponentMetaData">SSIS components metadata</param>
        /// <param name="eventType">SSIS event type</param>
        /// <param name="description">SSIS event description</param>
        public static void Fire(IDTSComponentMetaData100 ComponentMetaData, Type eventType, string description)
        {
            Fire(ComponentMetaData, eventType, 1, ComponentMetaData.Name + "|" + description);
        }


        /// <summary>
        /// fires an SSIS event
        /// </summary>
        /// <param name="ComponentMetaData">componets SSIS metadata</param>
        /// <param name="eventType">event type</param>
        /// <param name="sqlType">sql type (always</param>
        /// <param name="description">SSIS event description</param>
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

        /// <summary>
        /// event types
        /// </summary>
        public enum Type
        {
            Information = 0,
            Progress = 1,
            Warning = 2,
            Error = 3
        }
    }
}
