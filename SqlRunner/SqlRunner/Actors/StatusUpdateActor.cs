using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Akka.Actor;
using SqlRunner.Exceptions;
using SqlRunner.Messages.Abstract;
using SqlRunner.Messages.Dummy;
using SqlRunner.Messages.Status;
using SqlRunner.Models;
using SqlRunner.Utilities;

namespace SqlRunner.Actors
{
    internal class StatusUpdateActor : TypedActor,
        IHandle<UpdateRunStatusAsPickedUpRequestMessage>,
        IHandle<UpdateRunStatusAsProcessingRequestMessage>,
        IHandle<UpdateRunStatusAsCompletedRequestMessage>,
        IHandle<UpdateRunStatusAsErroredOutRequestMessage>
    {
        public void Handle(UpdateRunStatusAsCompletedRequestMessage message)
        {
            SimpleLogger.LogThisReallyCrappyMessage("Info", "Run completed");
            UpdateScriptRecordStatusInDatabase(message.ScriptId, RunStatusEnum.Completed);
            UpdateScriptActualRunDatetimeInDatabase(message);
            Sender.Tell(new GetToTheChopperMessage { ScriptId = message.ScriptId }, Self);
        }

        public void Handle(UpdateRunStatusAsErroredOutRequestMessage message)
        {
            SimpleLogger.LogThisReallyCrappyMessage("Info", "Run errored out");
            UpdateScriptRecordStatusInDatabase(message.ScriptId, RunStatusEnum.ErroredOut);
            UpdateScriptActualRunDatetimeInDatabase(message);
            UpdateErrorTextInDatabase(message);
            Sender.Tell(new GetToTheChopperMessage { ScriptId = message.ScriptId }, Self);
        }

        public void Handle(UpdateRunStatusAsPickedUpRequestMessage message)
        {
            SimpleLogger.LogThisReallyCrappyMessage("Info", "Run picked up");
            UpdateProcessingMachineProcessIdFieldsInDatabase(message);
            UpdateScriptRecordStatusInDatabase(message.ScriptId, RunStatusEnum.PickedUp);
            Sender.Tell(new GetToTheChopperMessage { ScriptId = message.ScriptId }, Self);
        }

        public void Handle(UpdateRunStatusAsProcessingRequestMessage message)
        {
            SimpleLogger.LogThisReallyCrappyMessage("Info", "Run processing");
            UpdateScriptRecordStatusInDatabase(message.ScriptId, RunStatusEnum.InProcess);
            Sender.Tell(new GetToTheChopperMessage { ScriptId = message.ScriptId }, Self);
        }

        public void UpdateScriptRecordStatusInDatabase(int scriptId, RunStatusEnum runStatus)
        {
            DatabaseUtil.RunFreeTextQueryInSqlRunner(Constants.UpdateRunStatusQuery, new List<SqlParameter>
            {
                new SqlParameter(Constants.RunStatusIdParam, SqlDbType.Int) {Value = runStatus},
                new SqlParameter(Constants.ScriptIdParam, SqlDbType.Int) {Value = scriptId}
            }, true);
        }

        public void UpdateScriptActualRunDatetimeInDatabase(AbstractScriptMessage message)
        {
            DatabaseUtil.RunFreeTextQueryInSqlRunner(Constants.UpdateActualRunQuery, new List<SqlParameter>
            {
                new SqlParameter(Constants.ActualRunDatetimeParamName, SqlDbType.DateTime) {Value = DateTime.Now},
                new SqlParameter(Constants.ScriptIdParam, SqlDbType.Int) {Value = message.ScriptId}
            }, true);
        }

        public void UpdateProcessingMachineProcessIdFieldsInDatabase(UpdateRunStatusAsPickedUpRequestMessage message)
        {
            DatabaseUtil.RunFreeTextQueryInSqlRunner(Constants.SetMachineInfoQuery, new List<SqlParameter>
            {
                new SqlParameter(Constants.ProcessingMachineParam, SqlDbType.NVarChar) {Value = Environment.MachineName},
                new SqlParameter(Constants.ProcessIdParam, SqlDbType.Int) {Value = Process.GetCurrentProcess().Id },
                new SqlParameter(Constants.ScriptIdParam, SqlDbType.Int) {Value= message.ScriptId}
            }, true);
        }

        public void UpdateErrorTextInDatabase(UpdateRunStatusAsErroredOutRequestMessage message)
        {
            DatabaseUtil.RunFreeTextQueryInSqlRunner(Constants.SetErrorTextQuery, new List<SqlParameter>
            {
                new SqlParameter(Constants.ErrorTextParam, SqlDbType.NVarChar) {Value = message.ErrorText},
                new SqlParameter(Constants.ScriptIdParam, SqlDbType.Int) {Value= message.ScriptId}
            }, true);
        }
    }
}