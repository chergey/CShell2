using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace CShell.Framework.Results
{
    public class RunCodeResult : ResultBase
    {
        [Import]
        public Workspace Workspace { get; set; }

        private readonly string _code;
        private readonly string _sourceFile;

        public RunCodeResult(string code)
        {
            this._code = code;
        }

        public RunCodeResult(string code, string sourceFile)
        {
            this._code = code;
            this._sourceFile = sourceFile;
        }

        public override void Execute(CoroutineExecutionContext context)
        {
            if(Workspace != null && Workspace.ReplExecutor != null)
            {
                Task.Run(() => Workspace.ReplExecutor.Execute(_code, _sourceFile))
                    .ContinueWith(t => OnCompleted(t.Exception));
            }
        }
    }
}
