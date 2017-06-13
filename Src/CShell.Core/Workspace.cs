﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Caliburn.Micro;
using CShell.Framework.Services;
using CShell.Util;

namespace CShell
{
    [Export]
    public sealed partial class Workspace : PropertyChangedBase
    {
        private readonly IShell _shell;
        private readonly IReplScriptExecutorFactory _replExecutorFactory;

        private IReplScriptExecutor _replExecutor;

        [ImportingConstructor]
        public Workspace(IShell shell, IReplScriptExecutorFactory replExecutorFactory)
        {
            this._shell = shell;
            this._replExecutorFactory = replExecutorFactory;
        }

        public IReplScriptExecutor ReplExecutor => _replExecutor;

        public string WorkspaceDirectory { get; private set; }


        public void SetWorkspaceDirectory(string dir)
        {
            if (!string.IsNullOrEmpty(WorkspaceDirectory))
            {
                //try to save the layout
                SaveLayout();
            }

            if(_replExecutor != null)
                _replExecutor.Reset();

            _replExecutor = null;
            WorkspaceDirectory = dir;

            if (!string.IsNullOrEmpty(WorkspaceDirectory))
            {
                WorkspaceDirectory = Path.GetFullPath(WorkspaceDirectory);
                //create executor
                //note: csx scripts for configuration and loading references is executed in the ReplScriptExecutor
                _replExecutor = _replExecutorFactory.Create(WorkspaceDirectory);
                //restore layout
                LoadLayout();
            }

            NotifyOfPropertyChange(() => WorkspaceDirectory);
            NotifyOfPropertyChange(() => ReplExecutor);
        }

       

        #region Helpers

        public static void CreateEmptyFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var templateFile = Path.Combine(exeDir, Constants.TemplatesPath, "Empty" + extension);
            var emptyFileText = "";
            if (File.Exists(templateFile))
                emptyFileText = File.ReadAllText(templateFile);
            File.WriteAllText(filePath, emptyFileText);
        }
        #endregion

    }//end class
}
