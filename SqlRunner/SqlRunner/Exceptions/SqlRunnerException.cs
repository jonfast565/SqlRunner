using System;

namespace SqlRunner.Exceptions
{
    internal class SqlRunnerException : Exception
    {
        public SqlRunnerException(string message) : base(message)
        {
        }

        public SqlRunnerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}