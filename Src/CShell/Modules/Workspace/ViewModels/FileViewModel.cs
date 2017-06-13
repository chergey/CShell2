﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using CShell.Framework.Results;
using CShell.Modules.Workspace.Results;
using CShell.Util;
using Caliburn.Micro;
using CShell.Framework;
using Microsoft.Win32;

namespace CShell.Modules.Workspace.ViewModels
{
    public class FileViewModel : TreeViewModel
    {
        protected readonly FileInfo fileInfo;

        public FileViewModel(string filePath)
            :this(new FileInfo(filePath))
        { }

        public FileViewModel(FileInfo info)
        {
            fileInfo = info;
            DisplayName = fileInfo.Name;
            IsEditable = true;
        }

        public FileInfo FileInfo => fileInfo;

        public virtual string ToolTip => RelativePath;

        public string RelativePath => PathHelper.ToRelativePath(Environment.CurrentDirectory, fileInfo.FullName);

        public string FileExtension => fileInfo.Extension;

        public override Uri IconSource
        {
            get
            {
                switch (FileExtension)
                {
                    case ".cshell":
                    case ".cs":
                    case ".csx":
                        return new Uri("pack://application:,,,/CShell;component/Resources/Icons/Icons.16x16.CSFile.png");
                    case ".txt":
                    case ".log":
                        return new Uri("pack://application:,,,/CShell;component/Resources/Icons/Icons.16x16.TextFileIcon.png");
                    default:
                        return new Uri("pack://application:,,,/CShell;component/Resources/Icons/Icons.16x16.MiscFiles.png");
                }
            }
        }

        public IEnumerable<IResult> Delete()
        {
            if (!IsEditable)
                return null;
            var result = MessageBox.Show("Are you sure you want to delete this file?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
                fileInfo.Delete();
            return null;
        }

        public IEnumerable<IResult> Rename()
        {
            if (!IsEditable)
                return null;
            IsInEditMode = true;
            return null;
        }

        protected override void EditModeFinished()
        {
            try
            {
                if (DisplayName != null && DisplayName != fileInfo.Name)
                    fileInfo.MoveTo(Path.Combine(fileInfo.DirectoryName, DisplayName));
                else
                    DisplayName = fileInfo.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.Substring(0, ex.Message.IndexOf(".") + 1), "Rename", MessageBoxButton.OK, MessageBoxImage.Warning);
                DisplayName = fileInfo.Name;
            }
        }

       

        public IEnumerable<IResult> AddFileReferences()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = CShell.Constants.AssemblyFileFilter;
            dialog.Multiselect = true;
            yield return Show.Dialog(dialog);
            if (dialog.FileNames != null && dialog.FileNames.Length > 0)
            {
                var docResult = new OpenDocumentResult(fileInfo.FullName);
                yield return docResult;
                yield return new AddReferencesResult(docResult.Document as ITextDocument, dialog.FileNames);
            }
        }

        public IEnumerable<IResult> AddGacReferences()
        {
            var windowSettings = new Dictionary<string, object> { { "SizeToContent", SizeToContent.Manual }, { "Width", 500.0 }, { "Height", 500.0 } };
            var dialog = new AssemblyGacViewModel();
            yield return Show.Dialog(dialog, windowSettings);
            var selectedAssemblies = dialog.SelectedAssemblies.Select(item => item.AssemblyName).ToArray();
            if (selectedAssemblies.Length > 0)
            {
                var docResult = new OpenDocumentResult(fileInfo.FullName);
                yield return docResult;
                yield return new AddReferencesResult(docResult.Document as ITextDocument, selectedAssemblies);
            }
        }

        public bool CanAddFileReferences => fileInfo.Extension.ToLower() == ".csx";

        public bool CanAddGacReferences => fileInfo.Extension.ToLower() == ".csx";
    }//end class
}
