﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using CShell.Framework;
using CShell.Framework.Menus;
using CShell.Framework.Results;
using CShell.Framework.Services;
using Caliburn.Micro;
using Microsoft.Win32;

namespace CShell.Modules.Shell
{
	[Export(typeof(IModule))]
	public class Module : ModuleBase
	{
        public Module()
	    {
	        Order = 1;
	    }

        public override void Start()
        {
		    var openWorkspace = new MenuItem("Open Workspace...", OpenWorkspace);
            var openFile = new MenuItem("_Open File...", OpenFile)
                .WithIcon("/Resources/Icons/Open.png")
                .WithGlobalShortcut(ModifierKeys.Control, Key.O);
            var closeFile = new MenuItem("_Close", CloseFile);
            var closeAllFiles = new MenuItem("Close All", CloseAllFiles);
            var save = new MenuItem("Save", Save)
		        .WithIcon("Resources/Icons/Icons.16x16.SaveIcon.png")
		        .WithGlobalShortcut(ModifierKeys.Control, Key.S);
		    var saveAs = new MenuItem("Save As...", SaveAs);
            var saveAll = new MenuItem("Save All", SaveAll)
                .WithIcon("Resources/Icons/Icons.16x16.SaveAllIcon.png")
                .WithGlobalShortcut(ModifierKeys.Control | ModifierKeys.Shift, Key.S);
		    var exit = new MenuItem("E_xit", Exit);
            
            //populate the menu
            MainMenu.First(item=>item.Name == "File")
                .Add(
                    openWorkspace,
                    openFile,
                    MenuItemBase.Separator,
                    closeFile,
                    closeAllFiles,
                    MenuItemBase.Separator,
                    save,
                    saveAs,
                    saveAll,
                    MenuItemBase.Separator,
                    exit
                );

            //populate the toolbar
            ToolBar.Add(
                    openFile,
                    save,
                    saveAll
                );
		}

        //private IEnumerable<IResult> NewWorkspace()
        //{
        //    var dialog = new SaveFileDialog();
        //    dialog.Filter = CShell.Constants.CShellFileTypes;
        //    dialog.DefaultExt = CShell.Constants.CShellFileExtension;
        //    yield return Show.Dialog(dialog);
        //    yield return new CloseWorkspaceResult();
        //    yield return new OpenWorkspaceResult(dialog.FileName);
        //}

        private IEnumerable<IResult> OpenWorkspace()
        {
            var folderResult = Show.FolderDialog();
            yield return folderResult;
            var folder = folderResult.SelectedFolder;
            yield return new ChangeWorkspaceResult(folder);
        }


	    private IEnumerable<IResult> NewFile()
        {
	        var dialog = new SaveFileDialog
	        {
	            Filter = CShell.Constants.FileTypes,
	            DefaultExt = CShell.Constants.DefaultExtension
	        };
	        yield return Show.Dialog(dialog);
            yield return Show.Document(dialog.FileName);
        }

        private IEnumerable<IResult> OpenFile()
        {
            var dialog = new OpenFileDialog {Filter = CShell.Constants.FileFilter};
            yield return Show.Dialog(dialog);
            yield return Show.Document(dialog.FileName);
        }

        private IEnumerable<IResult> CloseFile()
        {
            if(Shell.ActiveItem != null)
                Shell.ActiveItem.TryClose();
            yield break;
        }

        private IEnumerable<IResult> CloseAllFiles()
        {
            if (Shell.Documents != null)
            {
                var documents = this.Shell.Documents.ToList();
                var length = documents.Count;
                for (var i = 0; i < length; i++)
                {
                    this.Shell.CloseDocument(documents[i]);
                }
            }
            yield break;
        }

        private IEnumerable<IResult> Save()
        {
            var doc = Shell.ActiveItem as IDocument;
            if (doc != null)
            {
                yield return new SaveDocumentResult(doc);
            }
        }

        private IEnumerable<IResult> SaveAs()
        {
            var doc = Shell.ActiveItem as IDocument;
            if (doc != null)
            {
                var dialog = new SaveFileDialog();
                yield return Show.Dialog(dialog);
                yield return new SaveDocumentResult(doc, dialog.FileName);
            }
        }

        private IEnumerable<IResult> SaveAll()
        {
            return Shell.Documents.Select(doc => new SaveDocumentResult(doc));
        }


        private IEnumerable<IResult> Exit()
        {
            Shell.Close();
            yield break;
        }
	}
}