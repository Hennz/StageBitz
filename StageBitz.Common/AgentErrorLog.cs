using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace StageBitz.Common
{
    /// <summary>
    /// Error log class for stagebitz agent.
    /// </summary>
    public static class AgentErrorLog
    {
        const string errorLogName = "StageBitzAgent";
        const string errorLogSource = "StageBitzAgent";

        #region Error Handling

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public static void HandleException(Exception ex)
        {
            WriteToErrorLog(ex.StackTrace);

            if (ex.InnerException != null)
            {
                WriteToErrorLog(ex.InnerException.StackTrace);
            }
        }

        /// <summary>
        /// Writes to error log.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteToErrorLog(string message)
        {
            if (!EventLog.SourceExists(errorLogSource))
                EventLog.CreateEventSource(errorLogSource, errorLogName);

            EventLog.WriteEntry(errorLogSource, message, EventLogEntryType.Error);
        }

        #endregion
    }
}
