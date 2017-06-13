using System;
using System.Windows.Input;
using CShell.Framework.Services;

namespace CShell.Framework
{
	public abstract class Tool : LayoutItemBase, ITool
	{
		private ICommand _closeCommand;
		public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(p => IsVisible = false, p => true));

	    public abstract PaneLocation PreferredLocation { get; }
        public abstract Uri Uri { get; }

		public virtual Uri IconSource => null;

	    private bool _isVisible;
		public bool IsVisible
		{
			get { return _isVisible; }
			set
			{
				_isVisible = value;
				NotifyOfPropertyChange(() => IsVisible);
			}
		}

		protected Tool()
		{
			IsVisible = true;
		}
	}
}