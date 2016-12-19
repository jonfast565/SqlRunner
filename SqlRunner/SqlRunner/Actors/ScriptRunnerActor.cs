using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Akka.Actor;
using SqlRunner.Messages;
using SqlRunner.Messages.Status;
using SqlRunner.Models;
using SqlRunner.Utilities;

namespace SqlRunner.Actors
{
    internal class ScriptRunnerActor : TypedActor, IHandle<RunnableScriptObject>
    {
        public void Handle(RunnableScriptObject message)
        {
            SimpleLogger.LogThisReallyCrappyMessage("Info", "Handle script run request");
            var storageActor = ActorWrapper.GetUserActorSelection(ActorWrapper.StorageActorName);
            var scriptPullActor = ActorWrapper.GetUserActorSelection(ActorWrapper.ScriptPullActorName);
            var statusUpdateActor = ActorWrapper.GetUserActorSelection(ActorWrapper.StatusUpdateActorName);

            try
            {
                // pick it up!
                LogResultProcessing(message, statusUpdateActor);
                GetScriptContentsDataSetResult(message);
                LogResultSuccess(message, storageActor, scriptPullActor, statusUpdateActor);
            }
            catch (Exception e)
            {
                LogResultFailure(message, storageActor, statusUpdateActor, e);
            }
        }

        private static void GetScriptContentsDataSetResult(RunnableScriptObject message)
        {
            Action impersonatedAction = () =>
            {
                ExecuteSqlScriptAndOutputResult(message, true);
            };

            if (message.UseWindowsAuthentication)
            {
                var winImpersonator = new WindowsIdentityImpersonator();
                winImpersonator.ImpersonateUserAndRunAction(
                    message.Domain,
                    message.Username,
                    message.Password,
                    impersonatedAction
                    );
            }
            else
            {
                ExecuteSqlScriptAndOutputResult(message, false);
            }
        }

        private void ResetStatus(RunnableScriptObject message)
        {
            DatabaseUtil.RunFreeTextQueryInSqlRunner(Constants.ResetStatusQuery, new List<SqlParameter>
            {
                new SqlParameter(Constants.ScriptIdParam, SqlDbType.Int) { Value = message.RunnableScriptId }
            }, true);
        }

        private void LogResultSuccess(RunnableScriptObject message, ICanTell storageActor, ICanTell scriptPullActor, ICanTell statusUpdateActor)
        {
            storageActor.Tell(new DbResultMessage
            {
                RunnableScriptId = message.RunnableScriptId,
                ReturnMessage = "SUCCESS",
                ReturnValue = 0
            }, Self);

            var updateMessage = new UpdateRunStatusAsCompletedRequestMessage
            {
                ScriptId = message.RunnableScriptId
            };

            statusUpdateActor.Tell(updateMessage, Self);
            scriptPullActor.Tell(updateMessage, Self);
        }

        private void LogResultFailure(RunnableScriptObject message, ICanTell storageActor, 
            ICanTell statusUpdateActor, Exception e)
        {
            storageActor.Tell(new DbResultMessage
            {
                RunnableScriptId = message.RunnableScriptId,
                ReturnData = null,
                ReturnException = e,
                ReturnMessage = "FAILURE",
                ReturnValue = -1
            }, Self);

            statusUpdateActor.Tell(new UpdateRunStatusAsErroredOutRequestMessage
            {
                ScriptId = message.RunnableScriptId,
                ErrorText = e.ToString()
            }, Self);
        }

        private void LogResultProcessing(RunnableScriptObject message, ICanTell statusUpdateActor)
        {
            statusUpdateActor.Tell(new UpdateRunStatusAsProcessingRequestMessage 
            {
                ScriptId = message.RunnableScriptId
            }, Self);
        }

        private static void ExecuteSqlScriptAndOutputResult(RunnableScriptObject message, bool impersonatedContext)
        {
            var connectionString = message.BuildConnectionString(impersonatedContext);
            DatabaseUtil.RunArbitrarySqlScript(message.Contents, connectionString); 
        }
    }
}