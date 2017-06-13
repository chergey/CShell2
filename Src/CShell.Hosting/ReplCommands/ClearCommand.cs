using CShell.Framework.Services;
using CShell.Hosting.ReplCommands;
using ScriptCs.Contracts;

namespace CShell.Hosting.ReplCommands
{
    public class ClearCommand : IReplCommand
    {
        private readonly IReplOutput _replOutput;

        public ClearCommand(IReplOutput replOutput)
        {
            this._replOutput = replOutput;
        }

        public string CommandName => "clear";

        public string Description => "Clears all text from the REPL.";

        public object Execute(IRepl repl, object[] args)
        {
            _replOutput.Clear();
            return null;
        }
    }
}