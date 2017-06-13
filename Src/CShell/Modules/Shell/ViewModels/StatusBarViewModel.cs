using System;
using System.ComponentModel.Composition;
using CShell.Framework.Services;
using Caliburn.Micro;
using Execute = CShell.Framework.Services.Execute;

namespace CShell.Modules.Shell.ViewModels
{
	[Export(typeof(IStatusBar))]
	public class StatusBarViewModel : PropertyChangedBase, IStatusBar
	{
        private readonly object _syncRoot = new object();
	    private const string DefaultMessage = "Ready";

		private string _message;
		public string Message
		{
            get { lock(_syncRoot) return _message; }
		}

        public void UpdateMessage() => UpdateMessage(DefaultMessage);

	    public void UpdateMessage(string message)
        {
            lock (_syncRoot)
            {
                this._message = message;
            }
            Execute.OnUiThread(()=>NotifyOfPropertyChange(() => Message));
        }

	    private int _progress = 0;
        public int Progress
        {
            get { lock (_syncRoot) return _progress; }
        }

	    private bool _showingProgress;
        public bool ShowingProgress
        {
            get { lock (_syncRoot) return _showingProgress; }
        }

        public bool IndeterminateProgress
        {
            get { lock (_syncRoot) return _progress <= 0; }
        }

        public void UpdateProgress(bool running)
        {
            lock (_syncRoot)
            {
                _showingProgress = running;
                if (!running)
                    _progress = 0;
            }
            NotifyOfPropertyChange(() => Progress);
            NotifyOfPropertyChange(() => ShowingProgress);
            NotifyOfPropertyChange(() => IndeterminateProgress);
        }

        public void UpdateProgress(int progress)
        {
            var prog = progress;
            if (progress < 0)
                prog = 0;
            else if (progress > 100)
                prog = 100;

            lock (_syncRoot)
            {
                _showingProgress = true;
                this._progress = prog;
            }
            NotifyOfPropertyChange(() => Progress);
            NotifyOfPropertyChange(() => ShowingProgress);
            NotifyOfPropertyChange(() => IndeterminateProgress);
        }
    }
}