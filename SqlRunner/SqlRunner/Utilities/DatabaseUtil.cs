using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace SqlRunner.Utilities
{
    internal static class DatabaseUtil
    {
        public static DataSet RunFreeTextQuery(string query, string connectionString, 
            ICollection<SqlParameter> sqlParameters = null, bool withTransaction = false)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction t = null;
                if (withTransaction)
                {
                    t = connection.BeginTransaction(IsolationLevel.Serializable);
                }
                using (var command = new SqlCommand(query, connection, t))
                {
                    command.CommandType = CommandType.Text;
                    if (sqlParameters != null)
                    {
                        command.Parameters.AddRange(sqlParameters.ToArray());
                    }
                    var da = new SqlDataAdapter(command);
                    var ds = new DataSet();
                    da.Fill(ds);
                    t?.Commit();
                    return ds;
                }
            }
        }

        public static DataSet RunFreeTextQueryInSqlRunner(string query, ICollection<SqlParameter> sqlParameters = null, bool withTransaction = false)
        {
            return RunFreeTextQuery(query,
                ConfigurationManager.ConnectionStrings[Constants.SqlRunnerConnectionStringName].ConnectionString, sqlParameters, withTransaction);
        }

        public static void RunArbitrarySqlScript(string script, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var server = new Server(new ServerConnection(connection));
                var scripts = new StringCollection {script};
                //scripts.AddRange(script.Split(new [] { "GO" }, StringSplitOptions.None));
                server.ConnectionContext.ExecuteNonQuery(scripts);
            }
        }

        public static DataRow GetScalarResult(DataSet ds)
        {
            return ds.Tables[0].Rows[0];
        }
    }
}