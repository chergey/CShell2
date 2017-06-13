using CShell.Properties;

namespace CShell.Modules.Repl.Controls
{
    internal class CommandHistory
    {
        private int _currentPosn;
        private string _lastCommand;
        private readonly CommandQueue<string> _commandHistory = new CommandQueue<string>(Settings.Default.REPLBuffer);

        internal void Add(string command)
        {
            if(string.IsNullOrWhiteSpace(command))
                return;

            if (command != _lastCommand)
            {
                _commandHistory.Add(command);
                _lastCommand = command;
                _currentPosn = _commandHistory.Count;
            }
            else
            {
                _currentPosn++;
            }
        }

        internal bool DoesPreviousCommandExist() => _currentPosn > 0;

        internal bool DoesNextCommandExist() => _currentPosn < _commandHistory.Count;

        internal string GetPreviousCommand()
        {
            _lastCommand = _commandHistory[--_currentPosn];
            return _lastCommand;
        }

        internal string GetNextCommand()
        {
            if (_currentPosn == _commandHistory.Count - 1)
            {
                _currentPosn++;
                return "";
            }
            else
            { 
                _lastCommand = (string)_commandHistory[++_currentPosn];
                return LastCommand;
            }
        }

        internal string LastCommand => _lastCommand;

        internal string[] GetCommandHistory() => _commandHistory.ToArray();
    }
}
