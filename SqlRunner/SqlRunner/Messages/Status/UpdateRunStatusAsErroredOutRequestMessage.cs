using SqlRunner.Messages.Abstract;

namespace SqlRunner.Messages.Status
{
    internal class UpdateRunStatusAsErroredOutRequestMessage : AbstractScriptMessage
    {
        public string ErrorText { get; set; }
    }
}