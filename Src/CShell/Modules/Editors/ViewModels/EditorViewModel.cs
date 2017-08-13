using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CShell.Completion;
using CShell.Framework;
using CShell.Framework.Services;
using CShell.Modules.Editors.Controls;
using CShell.Modules.Editors.Views;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Execute = CShell.Framework.Services.Execute;

namespace CShell.Modules.Editors.ViewModels
{
	public class EditorViewModel : Document, ITextDocument
	{
	    private readonly CShell.Workspace _workspace;
	    private string _originalText;
		private string _path;
		private string _fileName;
		private bool _isDirty;
	    private CodeCompletionTextEditor _textEditor;
	    private EditorView _editorView;

	    private string _toAppend;
	    private string _toPrepend;

        public EditorViewModel(CShell.Workspace workspace)
	    {
	        _workspace = workspace;
	    }

	    public string File => _path;

	    public override Uri Uri { get; set; }

		public override bool IsDirty
		{
			get => _isDirty;
		    set
            {
                if (value == _isDirty)
                    return;

                _isDirty = value;
                if (_isDirty)
                    DisplayName = _fileName + "*";
                else
                    DisplayName = _fileName;
                NotifyOfPropertyChange(() => IsDirty);
                NotifyOfPropertyChange(() => DisplayName);
            }
		}

		public override void CanClose(System.Action<bool> callback)
		{
		    //callback(!IsDirty);
            if (!IsDirty)
            {
                callback(true);
                return;
            }

            Execute.OnUiThreadEx(() =>
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save this document before closing?" + Environment.NewLine + Uri.AbsolutePath, "Confirmation", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                    callback(true);
                }
                else if (result == MessageBoxResult.No)
                {
                    callback(true);
                }
                else
                {
                    // Cancel
                    callback(false);
                }
            });
		}

		public void Open(Uri uri)
		{
		    this.Uri = uri;
		    var decodedPath = Uri.UnescapeDataString(uri.AbsolutePath);
			this._path = Path.GetFullPath(decodedPath);
			_fileName = Path.GetFileName(_path);
		    DisplayName = _fileName;
		}

		protected override void OnViewLoaded(object view)
		{
            _editorView = (EditorView)view;
            _textEditor = _editorView.TextEditor;
            if(System.IO.File.Exists(_path))
                _textEditor.OpenFile(_path);
            _originalText = _textEditor.Text;

            _textEditor.TextChanged += delegate
			{
                IsDirty = string.CompareOrdinal(_originalText, _textEditor.Text) != 0;
			};

            //some other settings
		    var extension = Path.GetExtension(_path);
		    extension = extension == null ? "" : extension.ToLower();
		    _textEditor.ShowLineNumbers = true;
            _textEditor.SyntaxHighlighting = GetHighlighting(extension);

		    if (_workspace != null && _workspace.ReplExecutor?.DocumentCompletion != null && (extension == ".cs" || extension == ".csx"))
		    {
		        _textEditor.Completion = _workspace.ReplExecutor.DocumentCompletion;
		        _textEditor.ReplExecutor = _workspace.ReplExecutor;
		    }

            //if any outstanding text needs to be appended, do it now
		    if (_toAppend != null)
		    {
		        Append(_toAppend);
		        _toAppend = null;
		    }
            if (_toPrepend != null)
            {
                Prepend(_toPrepend);
                _toPrepend = null;
            }

            //debug to see what commands are available in the editor
            //var c = textEditor.TextArea.CommandBindings;
            //foreach (System.Windows.Input.CommandBinding cmd in c)
            //{
            //    var rcmd = cmd.Command as RoutedCommand;
            //    if(rcmd != null)
            //    {
            //        Debug.Print(rcmd.Name + "  "+ rcmd.InputGestures.ToString());
            //    }
            //}
        }

        public override void Save()
        {
            Execute.OnUiThreadEx(() =>
            {
                _textEditor.Save(_path);
                _originalText = _textEditor.Text;
                IsDirty = false;
            });
        }

        public override void SaveAs(string newFile)
        {
            Execute.OnUiThreadEx(() =>
            {
                _textEditor.Save(newFile);
                this._path = newFile;
                _fileName = Path.GetFileName(newFile);
                Uri = new Uri(System.IO.Path.GetFullPath(newFile));

                _originalText = _textEditor.Text;
                IsDirty = false;
                DisplayName = _fileName;
                NotifyOfPropertyChange(() => DisplayName);
            });
        }

        public string GetSelectionOrCurrentLine()
        {
            var code = _textEditor.SelectedText;
            int offsetLine;
            var doc = _textEditor.Document;

            // if there is no selection, just use the current line
            if (string.IsNullOrEmpty(code))
            {
                offsetLine = doc.GetLocation(_textEditor.CaretOffset).Line;
                var line = doc.GetLineByNumber(offsetLine);
                var lineText = doc.GetText(line.Offset, line.Length);
                code = lineText;
            }
            else
                offsetLine = doc.GetLocation(_textEditor.SelectionStart + _textEditor.SelectionLength).Line;

            _textEditor.TextArea.Caret.Line = offsetLine + 1;
            _textEditor.ScrollToLine(offsetLine + 1);
            return code;
        }

		public override bool Equals(object obj)
		{
			var other = obj as EditorViewModel;
		    return other != null && Uri == other.Uri;
		}

        private IHighlightingDefinition GetHighlighting(string fileExtension)
        {
            var def = HighlightingManager.Instance.GetDefinitionByExtension(fileExtension);
            //if the definition was not found try the custom extensions
            if(def == null)
            {
                switch (fileExtension)
                {
                    case ".cshell":
                    case ".csx":
                        def = HighlightingManager.Instance.GetDefinition("C#");
                        break;
                }
            }
            return def;

            //var resourceManager = IoC.Get<IResourceManager>();
            //resourceManager.GetBitmap("Resources/Icon.ico",
            //    Assembly.GetExecutingAssembly().GetAssemblyName());

            //using (Stream s = myAssembly.GetManifestResourceStream("MyHighlighting.xshd"))
            //{
            //    using (XmlTextReader reader = new XmlTextReader(s))
            //    {
            //        textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            //    }
            //}
        }

        #region ITextDocument
        public void Undo()
        {
            Execute.OnUiThreadEx(()=>_textEditor.Undo());
        }

        public void Redo()
        {
            Execute.OnUiThreadEx(() => _textEditor.Redo());
        }

        public void Cut()
        {
            Execute.OnUiThreadEx(() => _textEditor.Cut());
        }

        public void Copy()
        {
            Execute.OnUiThreadEx(() => _textEditor.Copy());
        }

        public void Paste()
        {
            Execute.OnUiThreadEx(() => _textEditor.Paste());
        }

        public void SelectAll()
        {
            Execute.OnUiThreadEx(() => _textEditor.SelectAll());
        }

        public void Select(int start, int length)
        {
            start = Math.Abs(start);
            length = Math.Abs(length);
            Execute.OnUiThreadEx(() =>
            {
                if (start > _textEditor.Document.TextLength)
                    start = _textEditor.Document.TextLength - 1;
                if (start + length > _textEditor.Document.TextLength)
                    length = _textEditor.Document.TextLength - start;
                _textEditor.Select(start, length);
            });
        }

        public void Comment()
        {
            Execute.OnUiThreadEx(() => _editorView.Comment());
        }

        public void Uncomment()
        {
            Execute.OnUiThreadEx(() => _editorView.Uncomment());
        }

	    public void Append(string text)
	    {
            //if the text editor is available append right now, otherwise wait until later
	        if (_textEditor != null)
	        {
	            Text = Text + text;
	        }
	        else
	        {
	            _toAppend += text;
	        }
	    }

	    public void Prepend(string text)
	    {
            if (_textEditor != null)
            {
                Text = text + Text;
            }
            else
            {
                _toPrepend += text;
            }
        }

	    public string Text
        {
            get
            {
                if (_textEditor == null)
                    throw new NullReferenceException("textEditor is not ready");
                var txt = "";
                Execute.OnUiThreadEx(() => txt = _textEditor.Text);
                return txt;
            }
            set
            {
                if (_textEditor == null)
                    throw new NullReferenceException("textEditor is not ready");
                Execute.OnUiThreadEx(() =>
                {
                    if (value == null)
                        value = "";
                    using(_textEditor.Document.RunUpdate())
                    {
                        _textEditor.Document.Text = value;
                    }
                });
            }
        }
        #endregion


       
    }
}