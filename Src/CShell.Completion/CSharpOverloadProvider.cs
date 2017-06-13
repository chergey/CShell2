using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.Editor;

namespace CShell.Completion
{
    public class CSharpOverloadProvider : INotifyPropertyChanged, IOverloadProvider, IParameterDataProvider
    {
        private readonly CSharpCompletionContext _context;
        private readonly int _startOffset;
        internal readonly IList<CSharpInsightItem> Items;
        private int _selectedIndex;

        public CSharpOverloadProvider(CSharpCompletionContext context, int startOffset, IEnumerable<CSharpInsightItem> items)
        {
            Debug.Assert(items != null);
            this._context = context;
            this._startOffset = startOffset;
            this._selectedIndex = 0;
            this.Items = items.ToList();

            Update(context);
        }

        public bool RequestClose { get; set; }

        public int Count => Items.Count;

        public object CurrentContent => Items[_selectedIndex].Content;

        public object CurrentHeader => Items[_selectedIndex].Header;

        public string CurrentIndexText => (_selectedIndex + 1).ToString() + " of " + this.Count.ToString();

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                if (_selectedIndex >= Items.Count)
                    _selectedIndex = Items.Count - 1;
                if (_selectedIndex < 0)
                    _selectedIndex = 0;
                OnPropertyChanged("SelectedIndex");
                OnPropertyChanged("CurrentIndexText");
                OnPropertyChanged("CurrentHeader");
                OnPropertyChanged("CurrentContent");
            }
        }

        public void Update(IDocument document, int offset)
        {
            var completionContext = new CSharpCompletionContext(document, offset, _context.ProjectContent, _context.OriginalNamespaces);
            Update(completionContext);
        }

        public void Update(CSharpCompletionContext completionContext)
        {
            var completionFactory = new CSharpCompletionDataFactory(completionContext.TypeResolveContextAtCaret, completionContext);
            var pce = new CSharpParameterCompletionEngine(
                completionContext.Document,
                completionContext.CompletionContextProvider,
                completionFactory,
                completionContext.ProjectContent,
                completionContext.TypeResolveContextAtCaret
            );

            var completionChar = completionContext.Document.GetCharAt(completionContext.Offset - 1);
            var docText = completionContext.Document.Text;
            Debug.Print("Update Completion char: '{0}'", completionChar);
            int parameterIndex = pce.GetCurrentParameterIndex(_startOffset, completionContext.Offset);
            if (parameterIndex < 0)
            {
                RequestClose = true;
                return;
            }
            else
            {
                if (parameterIndex > Items[_selectedIndex].Method.Parameters.Count)
                {
                    var newItem = Items.FirstOrDefault(i => parameterIndex <= i.Method.Parameters.Count);
                    SelectedIndex = Items.IndexOf(newItem);
                }
                if (parameterIndex > 0)
                    parameterIndex--; // NR returns 1-based parameter index
                foreach (var item in Items)
                {
                    item.HighlightParameter(parameterIndex);
                }
            }
        }

        #region IParameterDataProvider implementation
        int IParameterDataProvider.StartOffset => _startOffset;

        string IParameterDataProvider.GetHeading(int overload, string[] parameterDescription, int currentParameter)
        {
            throw new NotImplementedException();
        }

        string IParameterDataProvider.GetDescription(int overload, int currentParameter)
        {
            throw new NotImplementedException();
        }

        string IParameterDataProvider.GetParameterDescription(int overload, int paramIndex)
        {
            throw new NotImplementedException();
        }

        string IParameterDataProvider.GetParameterName(int overload, int currentParameter)
        {
            throw new NotImplementedException();
        }

        int IParameterDataProvider.GetParameterCount(int overload)
        {
            throw new NotImplementedException();
        }

        bool IParameterDataProvider.AllowParameterList(int overload)
        {
            throw new NotImplementedException();
        }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }
    }
}
