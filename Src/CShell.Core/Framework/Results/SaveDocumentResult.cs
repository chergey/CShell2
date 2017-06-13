using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace CShell.Framework.Results
{
    public class SaveDocumentResult : ResultBase
    {
        private IDocument _document;
        private string _newFile;

        public SaveDocumentResult(IDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            this._document = document;
        }

        public SaveDocumentResult(IDocument document, string newFile)
            :this(document)
        {
            this._newFile = newFile;
        }

        public override void Execute(CoroutineExecutionContext context)
        {
            Exception ex = null;
            try
            {
                if (!String.IsNullOrEmpty(_newFile))
                {
                    _document.SaveAs(_newFile);
                }
                else if (_document.IsDirty)
                {
                    _document.Save();
                }
            }
            catch(Exception e)
            {
                ex = e;
            }
            OnCompleted(ex);
        }
    }
}
