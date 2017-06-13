using System;
using System.ComponentModel.Composition;
using System.Linq;
using CShell.Framework.Services;
using Caliburn.Micro;

namespace CShell.Framework.Results
{
	public class OpenDocumentResult : OpenResultBase<IDocument>
	{
		private IDocument _document;
        private readonly Type _documentType;
		private readonly Uri _uri;

		[Import]
		private IShell _shell;

		public OpenDocumentResult(IDocument document)
		{
            this._document = document;
		}

		public OpenDocumentResult(string path)
		{
		    if (path == null) throw new ArgumentNullException(nameof(path));
		    _uri = new Uri(System.IO.Path.GetFullPath(path));
		}

	    public OpenDocumentResult(Uri uri)
	    {
	        this._uri = uri;
	    }

        public OpenDocumentResult(Type documentType)
		{
            this._documentType = documentType;
		}

	    public IDocument Document => _document;

	    public override void Execute(CoroutineExecutionContext context)
		{
	        if (_document == null)
	        {
	            _document = (_uri == null
	                ? (IDocument) IoC.GetInstance(_documentType, null)
	                : Shell.GetDoc(_uri));
	        }

	        if (_document == null)
			{
				OnCompleted(null);
				return;
			}

			if (SetData != null)
                SetData(_document);

			if (_onConfigure != null)
                _onConfigure(_document);

            _document.Deactivated += (s, e) =>
			{
				if (_onShutDown != null)
                    _onShutDown(_document);
			};

            _shell.OpenDocument(_document);

            OnCompleted(null);
        }

       
	}
}