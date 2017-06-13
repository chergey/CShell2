﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using CShell.Framework.Results;
using CShell.Modules.Workspace.Results;
using CShell.Util;
using Caliburn.Micro;
using Microsoft.Win32;

namespace CShell.Modules.Workspace.ViewModels
{
    public class FolderViewModel : TreeViewModel
    {
        protected DirectoryInfo directoryInfo;
        private readonly CShell.Workspace _workspace;

        protected FolderViewModel(string path, CShell.Workspace workspace)
            :this(new DirectoryInfo(path),workspace)
        { }

        public FolderViewModel(DirectoryInfo info, CShell.Workspace workspace)
        {
            directoryInfo = info;
            this._workspace = workspace;
            DisplayName = directoryInfo.Name;
            IsEditable = true;
        }

        public CShell.Workspace Workspace => _workspace;

        public DirectoryInfo DirectoryInfo => directoryInfo;

        public override Uri IconSource
        {
            get
            {
                if (IsExpanded)
                    return new Uri("pack://application:,,,/CShell;component/Resources/Icons/Folder.Open.png");
                else
                    return new Uri("pack://application:,,,/CShell;component/Resources/Icons/Folder.Closed.png");
            }
        }

        public string RelativePath => PathHelper.ToRelativePath(Environment.CurrentDirectory, directoryInfo.FullName);

        public string ToolTip => RelativePath;

        private BindableCollection<TreeViewModel> _children;
        public override IObservableCollection<TreeViewModel> Children => _children ?? (_children = LoadChildren());

        protected virtual BindableCollection<TreeViewModel> LoadChildren()
        {
            var filer = "";//workspace.Filter ?? "";
            var filterStrings = filer.Split().Select(WildcardToRegex).ToArray();
            var filters = filterStrings.Select(fs=> new Regex(fs, RegexOptions.IgnoreCase)).ToArray();

            var items = new BindableCollection<TreeViewModel>();
            try
            {
              var dirs = directoryInfo.GetDirectories();
              foreach (var dir in dirs)
              {
                  if(filters.Any(f => f.IsMatch(dir.Name)))
                      continue;
                  var folderVm = new FolderViewModel(dir, _workspace);
                  items.Add(folderVm);
              }
              var files = directoryInfo.GetFiles();
              foreach (var file in files)
              {
                  if (filters.Any(f => f.IsMatch(file.Name)))
                      continue;
                  var fileVm = new FileViewModel(file);
                  items.Add(fileVm);
              }
            }
            catch (UnauthorizedAccessException ) // Does not have access to the folder cannot iterate.
            { }

            return items;
        }


        public IEnumerable<IResult> AddNewFile()
        {
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = directoryInfo.FullName;
            dialog.Filter = CShell.Constants.FileTypes;
            dialog.DefaultExt = CShell.Constants.DefaultExtension;
            yield return Show.Dialog(dialog);
            yield return new AddFilesResult(dialog.FileName);
        }

        public IEnumerable<IResult> AddNewFolder()
        {
            var folderResult = Show.FolderDialog(directoryInfo.FullName);
            yield return folderResult;
        }

        public IEnumerable<IResult> Delete()
        {
            if (!IsEditable)
                return null;
            var result = MessageBox.Show("Are you sure you want to delete this folder and all its contents?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                directoryInfo.Delete(true);
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
                if (DisplayName != null && DisplayName != directoryInfo.Name)
                    directoryInfo.MoveTo(Path.Combine(directoryInfo.Parent.FullName, DisplayName));
                else
                    DisplayName = directoryInfo.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.Substring(0, ex.Message.IndexOf(".")+1), "Rename", MessageBoxButton.OK, MessageBoxImage.Warning);
                DisplayName = directoryInfo.Name;
            }
        }

        public static string WildcardToRegex(string pattern) => "^" + Regex.Escape(pattern).
            Replace(@"\*", ".*").
            Replace(@"\?", ".") + "$";
    }
}
