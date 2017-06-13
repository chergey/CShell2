using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Timers;
using System.Windows.Media;
using CShell.Framework;
using CShell.Framework.Services;
using CShell.Modules.Repl.Controls;
using CShell.Modules.Repl.Views;
using Caliburn.Micro;
using ScriptCs.Contracts;
using Execute = CShell.Framework.Services.Execute;

namespace CShell.Modules.Repl.ViewModels
{
    [Export(typeof(ReplViewModel))]
    [Export(typeof(IReplOutput))]
    [Export(typeof(ITool))]
    public class ReplViewModel : Tool, IReplOutput
    {
        private readonly Timer _timer;
        private IReplOutput _internalReplOutput;
        private IReplView _replView;

        [Import] private IShell _shell;

        [ImportingConstructor]
        public ReplViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);

            DisplayName = "C# Interactive";

            _timer = new Timer(100) {AutoReset = true};
           // _timer.Elapsed += TimerOnElapsed;
        }
        
        public override Uri IconSource => new Uri("pack://application:,,,/CShell;component/Resources/Icons/Output.png");

        public override PaneLocation PreferredLocation => PaneLocation.Bottom;

        public override Uri Uri => new Uri("tool://cshell/repl");

        protected override void OnViewLoaded(object view)
        {
            _replView = (IReplView) view;
            _internalReplOutput = _replView.GetReplOutput();

            _timer.Start();
            base.OnViewLoaded(view);
        }

        protected override void OnDeactivate(bool close)
        {
            if(close)
                _timer.Dispose();
        }

        private DateTime _lastTimeNoEvaluations;
        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if(!_internalReplOutput.IsEvaluating)
            {
                _lastTimeNoEvaluations = DateTime.Now;
                if(_shell.StatusBar.Message != "Ready")
                {
                    _shell.StatusBar.UpdateMessage();
                    _shell.StatusBar.UpdateProgress(false);
                }
            }
            else
            {
                var evaluatingTime = DateTime.Now - _lastTimeNoEvaluations;
                if(evaluatingTime.TotalSeconds > 0.5)
                {
                    _shell.StatusBar.UpdateMessage("Running...");
                    _shell.StatusBar.UpdateProgress(true);
                }
            }
        }


        #region IRepl wrapper implementaion
        public void Initialize(IReplScriptExecutor replExecutor)
        {
            Execute.OnUiThread(() => _internalReplOutput.Initialize(replExecutor));
        }

        public void EvaluateStarted(string input, string sourceFile)
        {
            Execute.OnUiThread(() => _internalReplOutput.EvaluateStarted(input, sourceFile));
        }

        public void EvaluateCompleted(ScriptResult result)
        {
            Execute.OnUiThread(() => _internalReplOutput.EvaluateCompleted(result));
        }

        public void Clear()
        {
            Execute.OnUiThread(()=>_internalReplOutput.Clear());
        }

        public bool IsEvaluating => _internalReplOutput.IsEvaluating;

        public void Write(string value)
        {
            Execute.OnUiThread(() => _internalReplOutput.Write(value));
        }

        public void WriteLine()
        {
            Execute.OnUiThread(() => _internalReplOutput.WriteLine());
        }

        public void WriteLine(string value)
        {
            Execute.OnUiThread(() => _internalReplOutput.WriteLine(value));
        }

        public void Write(string format, params object[] arg)
        {
            Execute.OnUiThread(() => _internalReplOutput.Write(format, arg));
        }

        public void WriteLine(string format, params object[] arg)
        {
            Execute.OnUiThread(() => _internalReplOutput.WriteLine(format, arg));
        }

        public int BufferLength
        {
            get { return _internalReplOutput.BufferLength; }
            set { Execute.OnUiThread(() => _internalReplOutput.BufferLength = value); }
        }

        public IEnumerable<string> SuppressedWarnings => _internalReplOutput.SuppressedWarnings;

        public void SuppressWarning(string warningCode) => _internalReplOutput.SuppressWarning(warningCode);

        public void ShowWarning(string warningCode) => _internalReplOutput.ShowWarning(warningCode);

        public void ResetColor()
        {
            Execute.OnUiThread(() => _internalReplOutput.ResetColor());
        }

        public bool ShowConsoleOutput
        {
            get { return _internalReplOutput.ShowConsoleOutput; }
            set { Execute.OnUiThread(()=>_internalReplOutput.ShowConsoleOutput = value); }
        }

        public string Font
        {
            get { return _internalReplOutput.Font; }
            set { Execute.OnUiThread(()=>_internalReplOutput.Font = value); }
        }

        public double FontSize
        {
            get { return _internalReplOutput.FontSize; }
            set { Execute.OnUiThread(()=>_internalReplOutput.FontSize = value); }
        }

        public System.Windows.Media.Color BackgroundColor
        {
            get { return _internalReplOutput.BackgroundColor; }
            set { Execute.OnUiThread(()=>_internalReplOutput.BackgroundColor = value); }
        }

        public System.Windows.Media.Color ResultColor
        {
            get { return _internalReplOutput.ResultColor; }
            set { Execute.OnUiThread(()=>_internalReplOutput.ResultColor = value); }
        }

        public System.Windows.Media.Color WarningColor
        {
            get { return _internalReplOutput.WarningColor; }
            set { Execute.OnUiThread(()=>_internalReplOutput.WarningColor = value); }
        }

        public System.Windows.Media.Color ErrorColor
        {
            get { return _internalReplOutput.ErrorColor; }
            set { Execute.OnUiThread(()=>_internalReplOutput.ErrorColor = value); }
        }

        public System.Windows.Media.Color TextColor
        {
            get { return _internalReplOutput.TextColor; }
            set { Execute.OnUiThread(()=>_internalReplOutput.TextColor = value); }
        }
        #endregion
       
    }//end class
}
