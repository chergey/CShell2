using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;
using CShell.Framework.Services;

namespace CShell.Framework.Results
{
    public class ChangeWorkspaceResult : ResultBase
    {
        private readonly string _workspaceDirectory;

        [Import]
        private Workspace _workspace;

        /// <summary>
        /// Creates a result that will open a workspace.
        /// </summary>
        public ChangeWorkspaceResult(string workspaceDirectory)
        {
            this._workspaceDirectory = workspaceDirectory;
        }

        public override void Execute(CoroutineExecutionContext context)
        {
            
            try
            {
                //some of this is synchronous which can mess up the UI (especially on startup), so we execute it on a seperate thread
                Task.Factory.StartNew(() => _workspace.SetWorkspaceDirectory(_workspaceDirectory))
                    .ContinueWith(t2 => OnCompleted(t2.Exception));
            }
            catch (Exception ex)
            {
                OnCompleted(ex);
            }
        }
    }
}
