namespace SqlRunner.Utilities
{
    internal static class Constants
    {
        public const string GetterRequestQuery =
            "SELECT TOP 1 * FROM RunnableScripts WHERE ActualRunDatetime IS NULL AND ProcessingMachine IS NULL AND ProcessId IS NULL AND RunStatusId IS NULL AND RequestedRunDatetime <= @CurrentDatetimeParam;";

        public const string GetterByScriptIdRequestQuery =
            "SELECT TOP 1 * FROM RunnableScripts Where RunnableScriptId = @ScriptIdParam;";

        public const string SetMachineInfoQuery =
            "UPDATE RunnableScripts Set ProcessingMachine = @ProcessingMachineParam, ProcessId = @ProcessIdParam WHERE RunnableScriptId = @ScriptIdParam;";

        public const string UpdateActualRunQuery =
            "UPDATE RunnableScripts SET ActualRunDatetime = @ActualRunDatetimeParam WHERE RunnableScriptId = @ScriptIdParam;";

        public const string UpdateRunStatusQuery =
            "UPDATE RunnableScripts SET RunStatusId = @RunStatusIdParam WHERE RunnableScriptId = @ScriptIdParam;";

        public const string SetErrorTextQuery =
            "UPDATE RunnableScripts SET ErrorText = @ErrorTextParam WHERE RunnableScriptId = @ScriptIdParam;";

        public const string ResetStatusQuery =
            "UPDATE RunnableScripts SET RunStatusId = NULL, ProcessingMachine = NULL, ProcessId = NULL, ActualRunDatetime = NULL WHERE RunnableScriptId = @ScriptIdParam;";

        public const string SqlRunnerConnectionStringName = "SqlRunnerConnectionString";
        public const string ProcessingMachineParam = "@ProcessingMachineParam";
        public const string ActualRunDatetimeParamName = "@ActualRunDatetimeParam";
        public const string ProcessIdParam = "@ProcessIdParam";
        public const string ScriptIdParam = "@ScriptIdParam";
        public const string RunStatusIdParam = "@RunStatusIdParam";
        public const string ErrorTextParam = "@ErrorTextParam";
        public const string CurrentDatetimeParam = "@CurrentDatetimeParam";

    }
}