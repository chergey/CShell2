using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CShell.Framework.Services;
using ScriptCs.Contracts;

namespace CShell.Hosting.ReplCommands
{
    public class HelpCommand : IReplCommand
    {
        private readonly IReplOutput _replOutput;

        public HelpCommand(IReplOutput replOutput)
        {
            this._replOutput = replOutput;
        }

        public string CommandName => "help";

        public string Description => "Displays this help screen.";

        public object Execute(IRepl repl, object[] args)
        {
            _replOutput.WriteLine("The following commands are available in the REPL:");
            foreach (var command in repl.Commands.OrderBy(x => x.Key))
            {
                _replOutput.WriteLine(string.Format(":{0,-15}{1,10}", command.Key, command.Value.Description));
            }
            return null;
        }
    }
}
