using System;
using System.Data;

namespace SqlRunner.Messages
{
    internal class DbResultMessage
    {
        public int RunnableScriptId { get; set; } 
        public DataSet[] ReturnData { get; set; }
        public Exception ReturnException { get; set; }
        public string ReturnMessage { get; set; }
        public int ReturnValue { get; set; }
    }
}