using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SqlRunner.Exceptions;
using SqlRunner.Utilities;

namespace SqlRunner.Models
{
    internal class RunnableScriptObject
    {
        public int RunnableScriptId { get; set; }
        public string Contents { get; set; }

        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Server { get; set; }
        public bool UseWindowsAuthentication { get; set; }

        public DateTime? RequestedRunDatetime { get; set; }
        public DateTime? ActualRunDatetime { get; set; }
        public string ProcessingMachine { get; set; }
        public int ProcessId { get; set; }
        public int RunStatusId { get; set; }

        private static RunnableScriptObject FromDataRow(DataRow dr)
        {
            var runnableScriptId = (int) dr["RunnableScriptId"];
            var contents = (string) dr["Contents"];
            var domain = (string) dr["Domain"];
            var username = (string) dr["Username"];
            var password = StringCipher.Decrypt((string) dr["HashedPassword"]);
            var database = dr["Database"] == DBNull.Value ? null : (string) dr["Database"];
            var server = (string) dr["Server"];
            var useWindowsAuthentication = (bool) dr["UseWindowsAuthentication"];
            var requestedRunDatetime = dr["RequestedRunDatetime"] == DBNull.Value
                ? default(DateTime)
                : (DateTime) dr["RequestedRunDatetime"];
            var actualRunDatetime = dr["ActualRunDatetime"] == DBNull.Value
                ? default(DateTime)
                : (DateTime) dr["ActualRunDatetime"];
            var processingMachine = dr["ProcessingMachine"] == DBNull.Value ? null : (string)dr["ProcessingMachine"];
            var processId = dr["ProcessId"] == DBNull.Value ? default(int) : (int) dr["ProcessId"];
            var runStatusId = dr["RunStatusId"] == DBNull.Value ? default(int) : (int) dr["RunStatusId"];

            return new RunnableScriptObject
            {
                RunnableScriptId = runnableScriptId,
                Contents = contents,
                Domain = domain,
                Username = username,
                Password = password,
                Database = database,
                Server = server,
                UseWindowsAuthentication = useWindowsAuthentication,
                RequestedRunDatetime = requestedRunDatetime,
                ActualRunDatetime = actualRunDatetime,
                ProcessingMachine = processingMachine,
                ProcessId = processId,
                RunStatusId = runStatusId
            };
        }

        private static ICollection<RunnableScriptObject> MapScriptDataSetToScriptObjects(DataSet scriptData)
        {
            var tableResult = scriptData.Tables[0];
            return (from DataRow row in tableResult.Rows select FromDataRow(row)).ToList();
        }

        public static ICollection<RunnableScriptObject> GetFirstScriptFromQueue()
        {
            return MapScriptDataSetToScriptObjects(GetData());
        }

        private static DataSet GetData()
        {
            try
            {
                return DatabaseUtil.RunFreeTextQueryInSqlRunner(Constants.GetterRequestQuery, new List<SqlParameter>
                {
                    new SqlParameter(Constants.CurrentDatetimeParam, SqlDbType.DateTime) { Value = DateTime.Now }
                } , true);
            }
            catch (SqlException e)
            {
                throw new SqlRunnerException("Getting runnable scripts list failed", e);
            }
        }

        public static RunnableScriptObject GetDataByScriptId(int scriptId)
        {
            try
            {
                return FromDataRow(
                    DatabaseUtil.GetScalarResult(
                        DatabaseUtil.RunFreeTextQueryInSqlRunner(Constants.GetterByScriptIdRequestQuery,
                    new List<SqlParameter>
                    {
                        new SqlParameter(Constants.ScriptIdParam, SqlDbType.Int) {Value = scriptId}
                    }, true)));
            }
            catch (SqlException e)
            {
                throw new SqlRunnerException("Getting script lock failed", e);
            }
        }

        public string BuildConnectionString(bool impersonatedContext)
        {
            var connectionString = !impersonatedContext
                ? $"Data Source={Server};Initial Catalog={Database ?? "master"};User Id={Username};Password={Password};"
                : $"Data Source={Server};Initial Catalog={Database ?? "master"};Trusted_Connection=Yes";
            return connectionString;
        }

        public bool ProcessInfoIsSame(RunnableScriptObject rs)
        {
            return ProcessingMachine == rs.ProcessingMachine
                   && ProcessId == rs.ProcessId;
        }
    }
}