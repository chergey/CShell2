using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace CShell
{
    public static partial class Shell
    {
        private static readonly Lazy<Workspace> WorkspaceLazy = new Lazy<Workspace>(() => IoC.Get<Workspace>());

        /// <summary>
        /// Gets the instance of the currently open workspace.
        /// </summary>
        public static Workspace Workspace => WorkspaceLazy.Value;
    }
}
