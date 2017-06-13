// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.NRefactory.Completion;

namespace CShell.Completion.DataItems
{
    class CompletionData : ICompletionData, ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData
    {
        protected CompletionData()
        { }

        public CompletionData(string text)
        {
            DisplayText = CompletionText = Description = text;
        }

        public string TriggerWord { get; set; }
        public int TriggerWordLength { get; set; }

        #region NRefactory ICompletionData implementation
        public CompletionCategory CompletionCategory { get; set; }
        public string DisplayText { get; set; }
        public virtual string Description { get; set; }
        public string CompletionText { get; set; }
        public DisplayFlags DisplayFlags { get; set; }

        public bool HasOverloads => _overloadedData.Count > 0;

        readonly List<ICompletionData> _overloadedData = new List<ICompletionData>();
        public IEnumerable<ICompletionData> OverloadedData => _overloadedData;

        public void AddOverload(ICompletionData data)
        {
            if (_overloadedData.Count == 0)
                _overloadedData.Add(this);
            _overloadedData.Add(data);
        }
        #endregion

        #region AvalonEdit ICompletionData implementation

        public System.Windows.Media.ImageSource Image { get; set; }

        public void Complete(TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs) => textArea.Document.Replace(completionSegment, this.CompletionText);

        public object Content => DisplayText;

        object ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Description => this.Description;

        private double _priority = 1;
        public virtual double Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public string Text => this.CompletionText;

        #endregion

        #region Equals, ToString, GetHashCode...
        public override string ToString() => DisplayText;

        public override bool Equals(object obj)
        {
            var other = obj as CompletionData;
            return other != null && DisplayText == other.DisplayText;
        }

        public override int GetHashCode() => DisplayText.GetHashCode();

        #endregion

        
    } //end class CompletionData
}
