using System.Windows.Controls;

namespace CShell.Modules.Repl.Views
{
	/// <summary>
	/// Interaction logic for OutputView.xaml
	/// </summary>
	public partial class OutputView : UserControl, IOutputView
	{
		public OutputView()
		{
			InitializeComponent();
		}

		public void ScrollToEnd() => OutputText.ScrollToEnd();

	    public void SetText(string text)
		{
			OutputText.Text = text;
		}
	}
}
