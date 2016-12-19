using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using SqlRunner.Exceptions;
using SqlRunner.Messages.Dummy;
using SqlRunner.Messages.Status;
using SqlRunner.Models;
using SqlRunner.Utilities;

namespace SqlRunner.Actors
{
    internal class ScriptPullActor : TypedActor, IHandle<SqlRunnerOptions>
    {
        public void Handle(SqlRunnerOptions message)
        {
            SimpleLogger.LogThisReallyCrappyMessage("Info", "Handling runner request");

            var runnerActorWrapper = ActorWrapper.GetUserActorSelection(ActorWrapper.RunnerActorName);
            var statusUpdateActor = ActorWrapper.GetUserActorSelection(ActorWrapper.StatusUpdateActorName);
            var scriptObjects = RunnableScriptObject.GetFirstScriptFromQueue();
            if (!scriptObjects.Any())
            {
                Sender.Tell(new ThereIsNoDataOnlyZuulMessage(), Self);
                return;
            }

            // double lock
            var script = scriptObjects.ElementAt(0);
            LogResultPickedUp(script, statusUpdateActor);
            var sameScript = RunnableScriptObject.GetDataByScriptId(script.RunnableScriptId);
            if (!sameScript.ProcessInfoIsSame(script))
            {
                runnerActorWrapper.Tell(script, Self);
                Sender.Tell(new GetToTheChopperMessage { ScriptId = script.RunnableScriptId }, Self);
            }
            else
            {
                Sender.Tell(new ThereIsNoDataOnlyZuulMessage(), Self);
            }
        }

        private void LogResultPickedUp(RunnableScriptObject message, ICanTell statusUpdateActor)
        {
            Task t = statusUpdateActor.Ask(new UpdateRunStatusAsPickedUpRequestMessage
            {
                ScriptId = message.RunnableScriptId
            });
            t.Wait();
        }
    }
}