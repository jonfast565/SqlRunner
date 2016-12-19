using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using CommandLine;
using Microsoft.SqlServer.Management.Smo;
using SqlRunner.Actors;
using SqlRunner.Messages;
using SqlRunner.Messages.Dummy;
using SqlRunner.Models;
using SqlRunner.Utilities;

namespace SqlRunner
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Console.WriteLine(Header.GetHeader());
            return Parser.Default
                .ParseArguments<SqlRunnerOptions>(args)
                .MapResult(DriveWithCommandLineArguments, errs => -1);
        }

        private static int DriveWithCommandLineArguments(SqlRunnerOptions opts)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var pipelineTask = RunScriptPipeline(opts, cancellationTokenSource);
            pipelineTask.Wait(cancellationTokenSource.Token);
            return 0;
        }

        private static async Task RunScriptPipeline(SqlRunnerOptions opts, CancellationTokenSource cancellationTokenSource)
        {
            if (opts.AsDaemonProcess)
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    await RunScriptPipelineOnceFromCommandLine(opts, cancellationTokenSource);
                    await Task.Delay(2000);
                }
                return;
            }
            await RunScriptPipelineOnceFromCommandLine(opts, cancellationTokenSource);
        }

        private static async Task RunScriptPipelineOnceFromCommandLine(SqlRunnerOptions opts, CancellationTokenSource cancellationTokenSource)
        {
            var getterActor = ActorWrapper.GetUserActorSelection(ActorWrapper.ScriptPullActorName);
            var storageActor = ActorWrapper.GetUserActorSelection(ActorWrapper.StorageActorName);
            var queueResult = await getterActor.Ask(opts);
            var msg = queueResult as GetToTheChopperMessage;
            if (msg != null)
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var result = await storageActor.Ask(new DbResultRequestMessage { ScriptId = msg.ScriptId });
                    var yesResult = result as DbResultMessage;
                    if (yesResult != null)
                    {
                        break;
                    }
                    await Task.Delay(2000);
                }
            }
        }
    }
}