using Akka.Actor;
using Akka.Configuration;

namespace SqlRunner.Actors
{
    internal static class ActorWrapper
    {
        public const string ActorSystemName = "SqlRunnerActors";
        public const string ScriptPullActorName = "Puller";
        public const string RunnerActorName = "Runner";
        public const string StorageActorName = "Storage";
        public const string StatusUpdateActorName = "Status";
        public const string UserActorNamespacePrefix = "user/";
        private const string HoconActorSystemConfiguration = @"
akka {
  actor {
    serializers {
      wire = ""Akka.Serialization.WireSerializer, Akka.Serialization.Wire""
    }
    serialization-bindings {
      ""System.Object"" = wire
    }
  }
}
";
        private static readonly ActorSystem Actors;

        static ActorWrapper()
        {
            Actors = BuildActorSystem();
        }

        private static ActorSystem BuildActorSystem()
        {
            var actorSystem = ActorSystem.Create(ActorSystemName,
                ConfigurationFactory.ParseString(HoconActorSystemConfiguration));
            actorSystem.ActorOf<ScriptPullActor>(ScriptPullActorName);
            actorSystem.ActorOf<ScriptRunnerActor>(RunnerActorName);
            actorSystem.ActorOf<ResultStorageActor>(StorageActorName);
            actorSystem.ActorOf<StatusUpdateActor>(StatusUpdateActorName);
            return actorSystem;
        }

        public static ActorSelection GetUserActorSelection(string name)
        {
            return Actors.ActorSelection(UserActorNamespacePrefix + name);
        }
    }
}