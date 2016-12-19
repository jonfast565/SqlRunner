using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using SqlRunner.Messages;
using SqlRunner.Messages.Dummy;
using SqlRunner.Utilities;

namespace SqlRunner.Actors
{
    internal class ResultStorageActor : TypedActor, IHandle<DbResultRequestMessage>, IHandle<DbResultMessage>
    {
        public ResultStorageActor()
        {
            ResultStorage = new List<DbResultMessage>();
        }

        public List<DbResultMessage> ResultStorage { get; }

        public void Handle(DbResultMessage message)
        {
            SimpleLogger.LogThisReallyCrappyMessage("Info", "Handling store result request");
            ResultStorage.Add(message);
        }

        public void Handle(DbResultRequestMessage message)
        {
            SimpleLogger.LogThisReallyCrappyMessage("Info", "Handling result request");
            var result = ResultStorage.FirstOrDefault(x => x.RunnableScriptId == message.ScriptId);
            if (result != null)
            {
                Sender.Tell(result, Self);
                return;
            }
            Sender.Tell(new ThereIsNoDataOnlyZuulMessage(), Self);
        }
    }
}