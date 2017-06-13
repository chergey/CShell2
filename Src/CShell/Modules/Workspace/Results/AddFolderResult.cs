using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CShell.Framework.Results;
using CShell.Modules.Workspace.ViewModels;

namespace CShell.Modules.Workspace.Results
{
    public class AddFolderResult : ResultBase
    {
        private readonly string _folder;

        public AddFolderResult(string folder)
        {
            this._folder = folder;
        }

        public override void Execute(Caliburn.Micro.CoroutineExecutionContext context)
        {}
    }
}
