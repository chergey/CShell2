using System.Collections.Generic;
using Microsoft.Win32;

namespace CShell.Framework.Results
{
	public static class Show
	{
        public static ShowDialogResult Dialog(object dialogViewModel, IDictionary<string, object> setting = null) => new ShowDialogResult(dialogViewModel){Settings = setting};

	    public static ShowDialogResult Dialog<TViewModel>(IDictionary<string, object> setting = null) => new ShowDialogResult(typeof(TViewModel)){Settings = setting};

	    public static ShowCommonDialogResult Dialog(CommonDialog commonDialog) => new ShowCommonDialogResult(commonDialog);

	    public static ShowFolderDialogResult FolderDialog(string selectedFolder = null) => new ShowFolderDialogResult(selectedFolder);

	    public static ShowToolResult<TTool> Tool<TTool>()
			where TTool : ITool => new ShowToolResult<TTool>();

	    public static ShowToolResult<TTool> Tool<TTool>(TTool tool)
			where TTool : ITool => new ShowToolResult<TTool>(tool);

	    public static OpenDocumentResult Document(IDocument document) => new OpenDocumentResult(document);

	    public static OpenDocumentResult Document(string path) => new OpenDocumentResult(path);

	    public static OpenDocumentResult Document<T>()
				where T : IDocument => new OpenDocumentResult(typeof(T));
	}
}