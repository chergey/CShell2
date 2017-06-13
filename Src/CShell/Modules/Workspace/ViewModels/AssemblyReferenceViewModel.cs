using CShell.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CShell.Modules.Workspace.ViewModels
{
    public class AssemblyReferenceViewModel : TreeViewModel
    {
        private readonly string _filePath;
        private readonly IReplScriptExecutor _replExecutor;
        private readonly Assembly _assembly;

        public AssemblyReferenceViewModel(string filePath, IReplScriptExecutor replExecutor)
        {
            if (filePath.EndsWith(".dll") || filePath.EndsWith(".exe"))
            {
                this._filePath = filePath; //PathHelper.ToRelativePath(replExecutor.WorkspaceDirectory, filePath);
                _assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(filePath));
                FullPath = PathHelper.ToAbsolutePath(Environment.CurrentDirectory, this._filePath);
                Available = File.Exists(FullPath);
            }
            else
            {
                this._filePath = filePath;
                _assemblyName = new AssemblyName(filePath);
                Available = true;
            }

            this._replExecutor = replExecutor;
        }

        public AssemblyReferenceViewModel(Assembly assembly, IReplScriptExecutor replExecutor)
        {
            this._assembly = assembly;
            _assemblyName = assembly.GetName();
            Available = true;
            this._replExecutor = replExecutor;
        }

        public override string DisplayName
        {
            get { return _assemblyName.Name; }
            set
            { }
        }

        public string FilePath => _filePath;

        public string FullPath { get; private set; }

        public bool Available { get; private set; }

        private bool _removable = true;
        public bool Removable
        {
            get { return _removable; }
            set { _removable = value; }
        }

        private AssemblyName _assemblyName;
        public AssemblyName AssemblyName => _assemblyName;

        public override Uri IconSource => new Uri("pack://application:,,,/CShell;component/Resources/Icons/Icons.16x16.Reference.png");


        public string ToolTip => _filePath;

        public void Remove()
        {
            if(_assembly != null)
                _replExecutor.RemoveReferences(_assembly);
            else
                _replExecutor.RemoveReferences(_filePath);
        }
    }
}
