using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using CShell.Framework;
using CShell.Framework.Results;
using CShell.Framework.Services;
using CShell.Properties;
using Caliburn.Micro;

namespace CShell.Modules.Workspace.ViewModels
{
	[Export]
    [Export(typeof(ITool))]
    public class WorkspaceViewModel : Tool
	{
	    private readonly IShell _shell;
        private readonly CShell.Workspace _workspace;

	    [ImportingConstructor]
        public WorkspaceViewModel(CShell.Workspace workspace)
        {
          //  this._shell = _shell;
            DisplayName = "Workspace Explorer";
	        this._workspace = workspace;
            this._workspace.PropertyChanged += WorkspaceOnPropertyChanged;
         //   WorkspaceOnPropertyChanged(null,new PropertyChangedEventArgs("WorkspaceDirectory"));
        }

	    #region Display
        private TreeViewModel _tree;
        public TreeViewModel Tree => _tree;

	    public override PaneLocation PreferredLocation => PaneLocation.Left;

	    public override Uri IconSource => new Uri("pack://application:,,,/CShell;component/Resources/Icons/FileBrowser.png");

	    public override Uri Uri => new Uri("tool://cshell/workspace");

	    #endregion

        private void WorkspaceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "WorkspaceDirectory")
            {   //teardown the current workspace
                if (_tree != null)
                {
                    _tree.Dispose();
                    _tree = null;
                    NotifyOfPropertyChange(() => Tree);
                }

                if (_workspace.WorkspaceDirectory != null && Directory.Exists(_workspace.WorkspaceDirectory))
                {
                    _tree = new TreeViewModel();

                    //add the assembly references
                    var refs = new AssemblyReferencesViewModel(_workspace.ReplExecutor);
                    _tree.Children.Add(refs);

                    //add the file tree
                    //var files = new FileReferencesViewModel(workspace.Files, null);
                    var files = new FolderRootViewModel(_workspace.WorkspaceDirectory, _workspace);
                    _tree.Children.Add(files);

                    NotifyOfPropertyChange(() => Tree);

                    Settings.Default.LastWorkspace = _workspace.WorkspaceDirectory;
                    Settings.Default.Save();
                }
            }
        }

        public IEnumerable<IResult> Open(object node)
        {
            var fileVm = node as FileViewModel;
            if(fileVm != null)
                yield return Show.Document(fileVm.RelativePath);
  
        }

        public IEnumerable<IResult> Selected(object node)
        {
            yield break;
        }
    }
}