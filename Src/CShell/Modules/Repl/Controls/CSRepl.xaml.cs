using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using CShell.Completion;
using CShell.Framework.Services;
using CShell.Hosting;
using CShell.Util;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.NRefactory.Editor;
using ScriptCs.Contracts;

namespace CShell.Modules.Repl.Controls
{
    public enum TextType
    {
        Output,
        Warning,
        Error,
        Repl,
        None,
    }

    /// <summary>
    /// Interaction logic for CommandLineControl.xaml
    /// </summary>
    public partial class CsRepl : UserControl, IReplOutput
    {
        private CsReplTextEditor _textEditor;

        private IReplScriptExecutor _replExecutor;
        private readonly CommandHistory _commandHistory;

        private bool _executingInternalCommand;
        private string _partialCommand = "";
        private int _evaluationsRunning;
        private IVisualLineTransformer[] _initialTransformers;

        CompletionWindow _completionWindow;
        OverloadInsightWindow _insightWindow;

        public CsRepl()
        {
            InitializeComponent();

            _textEditor = new CsReplTextEditor {FontFamily = new FontFamily("Consolas")};
            var convertFrom = new FontSizeConverter().ConvertFrom("10pt");
            if (convertFrom != null) _textEditor.FontSize = (double)convertFrom;
            _textEditor.TextArea.PreviewKeyDown += TextAreaOnPreviewKeyDown;
            _textEditor.IsEnabled = false;
            _textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            _textEditor.FileName = "repl.csx";
            _textEditor.Repl = this;
            this.Content = _textEditor;

            _commandHistory = new CommandHistory();

            var errorStream = new ConsoleStream(TextType.Error, Write);
            var errorWriter = new StreamWriter(errorStream) {AutoFlush = true};
            Console.SetError(errorWriter);

            var stdoutStream = new ConsoleStream(TextType.Output, Write);
            var stdoutWriter = new StreamWriter(stdoutStream) {AutoFlush = true};
            Console.SetOut(stdoutWriter);

            ShowConsoleOutput = true;
            ResetColor();

            //supress duplicate using warnings
            SuppressWarning("CS0105");

            //clears the console and prints the headers
            // when clearing the initial transormers are removed too but we want to keep them
            _initialTransformers = _textEditor.TextArea.TextView.LineTransformers.ToArray();
            Clear();
        }

        internal IReplScriptExecutor ReplExecutor => _replExecutor;

        #region IReplOutput Interface Implementation (key parts)
        public void Initialize(IReplScriptExecutor replExecutor)
        {
            //unhook old executor
            this._replExecutor = replExecutor;
            this._textEditor.Completion = replExecutor.ReplCompletion;
            Clear();
            _textEditor.IsEnabled = true;
        }

        private string _currrentInput;
        private string _currentSourceFile;

        public void EvaluateStarted(string input, string sourceFile)
        {
            _currrentInput = input;
            _currentSourceFile = sourceFile;

            if (!_executingInternalCommand)
            {
                if (!IsEvaluating)
                {
                    ClearLine();
                    WriteLine();
                }
                _evaluationsRunning++;
                var source = sourceFile != null ? System.IO.Path.GetFileName(sourceFile) : "unknown source";
                WriteLine("[Evaluating external code (" + source + ")]", TextType.Repl);
            }
        }

        public void EvaluateCompleted(ScriptResult result)
        {
            if (!result.IsCompleteSubmission)
            {
                _partialCommand = _currrentInput;
                _prompt = _promptIncomplete;
            }
            else
            {
                _partialCommand = "";
                _prompt = _promptComplete;
            }

            if (result.HasErrors())
            {
                WriteLine(string.Join(Environment.NewLine, result.GetMessages()), TextType.Error);
            }
            if (result.HasWarnings())
            {
                var msgs = result.GetMessages();
                var warnings = FilterWarnings(msgs).ToList();
                if (warnings.Any())
                    WriteLine(string.Join(Environment.NewLine, warnings), TextType.Warning);
            }

            if (result.ReturnValue != null)
                WriteLine(ToPrettyString(result.ReturnValue));

            _executingInternalCommand = false;
            _evaluationsRunning--;
            if (!IsEvaluating)
            {
                WritePrompt();
            }
        }

        public bool IsEvaluating => _evaluationsRunning > 0;

        private readonly HashSet<string> _suppressedWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public IEnumerable<string> SuppressedWarnings => _suppressedWarnings;

        public void SuppressWarning(string warningCode)
        {
            if(string.IsNullOrEmpty(warningCode))
                return;
            if (!_suppressedWarnings.Contains(warningCode))
                _suppressedWarnings.Add(warningCode);
        }
        public void ShowWarning(string warningCode)
        {
            if (string.IsNullOrEmpty(warningCode))
                return;
            if (_suppressedWarnings.Contains(warningCode))
                _suppressedWarnings.Remove(warningCode);
        }

        private IEnumerable<string> FilterWarnings(IEnumerable<string> warnings)
        {
            return warnings.Where(warning => !_suppressedWarnings.Contains(GetWarningCode(warning)));
        }

        private string GetWarningCode(string warning)
        {
            var warningStart = warning.IndexOf("warning", StringComparison.OrdinalIgnoreCase);
            if (warningStart >= 0)
            {
                var start = warningStart + "warning".Length;
                var warningEnd = warning.IndexOf(':', start);
                if (warningEnd > 0 && warningEnd > start)
                {
                    var code = warning.Substring(start, warningEnd - start);
                    return code.Trim();
                }
            }
            return "";
        }

        public bool ShowConsoleOutput { get; set; }

        public void Clear()
        {
            _textEditor.Text = string.Empty;
            //so that the previous code highligting is cleared too
            _textEditor.TextArea.TextView.LineTransformers.Clear();
            foreach (var visualLineTransformer in _initialTransformers)
                _textEditor.TextArea.TextView.LineTransformers.Add(visualLineTransformer);


            WriteLine("CShell REPL (" + Assembly.GetExecutingAssembly().GetName().Version + ")", TextType.Repl);

            if (_replExecutor != null)
            {
                WriteLine("Enter C# code to be evaluated or enter \":help\" for more information.", TextType.Repl);
                WritePrompt();
            }
            else
            {
                WriteLine("No workspace open, open a workspace to use the REPL.", TextType.Warning);
            }

        }

        //TODO: implement repl buffer!
        public int BufferLength { get; set; }
        #endregion

        #region Handle REPL Input
        private void CommandEntered(string command)
        {
            Debug.WriteLine("Command: " + command);

            if(_replExecutor == null)
                throw new InvalidOperationException("Repl executor cannot be null when entering commands.");
            WriteLine();
            _commandHistory.Add(command);
            _executingInternalCommand = true;
            _evaluationsRunning++;
            var input = _partialCommand + Environment.NewLine + command;
            input = input.Trim();
            //todo: call execute ASYNC
            Task.Run(()=>_replExecutor.Execute(input));
        }

        private void ShowPreviousCommand()
        {
            if(_commandHistory.DoesPreviousCommandExist())
            {
                ClearLine();
                Write(_commandHistory.GetPreviousCommand(), TextType.None);
            }
        }

        private void ShowNextCommand()
        {
            if (_commandHistory.DoesNextCommandExist())
            {
                ClearLine();
                Write(_commandHistory.GetNextCommand(), TextType.None);
            }
        }
        #endregion

        #region TextEditor Events
        private void TextAreaOnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            //Debug.WriteLine("TextArea PreviewKeyDown: " + keyEventArgs.Key);
            var key = keyEventArgs.Key;

            //allow copy or interrup
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C)) ||
                (Keyboard.IsKeyDown(Key.RightCtrl) && Keyboard.IsKeyDown(Key.C)))
            {
                if (IsEvaluating)
                {
                    WriteLine("[Interrupting]", TextType.Repl);
                    _replExecutor.Terminate();//TODO: is this the right method?
                }
                return;
            }

            if(IsEvaluating)
            {
                WriteLine("[Evaluation is running, press 'ctrl+c' to interrupt]", TextType.Repl);
                keyEventArgs.Handled = true;
                return;
            }

            if(key == Key.Enter)
            {
                var command = GetCurrentLineText();
                CommandEntered(command);
                keyEventArgs.Handled = true;
                return;
            }
            if(key == Key.Up)
            {
                ShowPreviousCommand();
                keyEventArgs.Handled = true;
                return;
            }
            if (key == Key.Down)
            {
                ShowNextCommand();
                keyEventArgs.Handled = true;
                return;
            }
            if (key == Key.Back || key == Key.Left)
            {
                if(IsCaretAtPrompt())
                {
                    keyEventArgs.Handled = true;
                    return;
                }
            }
            if (key == Key.End)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (_textEditor.CaretOffset >= PromptOffset)
                    {
                        if (_textEditor.CaretOffset > _textEditor.SelectionStart)
                        {
                            // Caret is after selection start - extend selection to end of line.
                            _textEditor.SelectionLength = Doc.TextLength - _textEditor.SelectionStart;
                        }
                        else
                        {
                            // Caret is at selection start (or no selection has been made) - select
                            // from end of current selection (if any) to end of line (if there's anything
                            // left on the line to select).
                            int selLen = _textEditor.SelectionLength;
                            _textEditor.SelectionLength = 0;
                            if (_textEditor.SelectionStart + selLen < Doc.TextLength)
                            { 
                                _textEditor.SelectionStart = _textEditor.SelectionStart + selLen;
                                _textEditor.SelectionLength = Doc.TextLength - _textEditor.SelectionStart;
                            }
                        }
                    }
                }
                else
                {
                    // For some reason the selection isn't cleared when the user presses the end key and
                    // the whole line is selected. It works when the selection length is less than the
                    // length of the line though... Workaround:
                    _textEditor.SelectionLength = 0;
                }
                MoveCaretToEnd();
                keyEventArgs.Handled = true;
                return;
            }
            if (key == Key.Home)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (_textEditor.CaretOffset > PromptOffset)
                    {
                        if (_textEditor.CaretOffset == _textEditor.SelectionStart)
                        {
                            // Caret is at selection start (or no selection has been made) - select/extend to start of line.
                            int selLen = _textEditor.SelectionStart - PromptOffset + _textEditor.SelectionLength;
                            _textEditor.SelectionStart = PromptOffset;
                            _textEditor.SelectionLength = selLen;
                        }
                        else
                        {
                            // Caret is after selection start - select from start of line to 
                            // beginning of current selection.
                            int selLen = _textEditor.SelectionStart - PromptOffset;
                            _textEditor.SelectionStart = PromptOffset;
                            _textEditor.SelectionLength = selLen;
                        }
                    }
                }
                else
                {
                    // For some reason the selection isn't cleared when the user presses the home key and
                    // the whole line is selected. It works when the selection length is less than the
                    // length of the line though... Workaround:
                    _textEditor.SelectionLength = 0;
                }
                MoveCaretToAfterPrompt();
                keyEventArgs.Handled = true;
                return;
            }

            if (!IsCaretAtWritablePosition())
            {
                keyEventArgs.Handled = true;
            }
            else
            {
                //it's possible to select more than the current line with the mouse, but the cursor ends at the current line
                // in this case when the selection is edited, it needs to be changed only to the current line.
                if (IsSelectionBeforePrompOffset())
                    SelectCurrentLineOnly();

                //if Crtl+x is pressed and no text is selected we need to select only the text after the prompt
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.X)) ||
                    (Keyboard.IsKeyDown(Key.RightCtrl) && Keyboard.IsKeyDown(Key.X)))
                {
                    if (_textEditor.SelectionLength == 0)
                        SelectCurrentLine();
                    if (IsCaretAtPrompt())
                        keyEventArgs.Handled = true;
                }
            }
        }

        internal ICSharpCode.NRefactory.Editor.IDocument GetCompletionDocument(out int offset)
        {
            var lineText = GetCurrentLineText();
            var line = Doc.GetLineByOffset(Offset);
            offset = Offset - line.Offset - _prompt.Length;

            var vars = string.Join(Environment.NewLine, ReplExecutor.GetVariables().Select(v => v + ";"));
            var code = vars + lineText;
            offset += vars.Length;
            var doc = new ReadOnlyDocument(new ICSharpCode.NRefactory.Editor.StringTextSource(code), _textEditor.FileName);
            return doc;
        }
        #endregion

        #region TextEditor Helpers
        private string _promptComplete = " > ";
        private string _promptIncomplete = "   ";
        private string _prompt = " > ";
        private TextDocument Doc => _textEditor.Document;

        private int Offset => _textEditor.CaretOffset;

        private int PromptOffset
        {
            get
            {
                var lastLine = Doc.GetLineByNumber(Doc.LineCount);
                return lastLine.Offset + _prompt.Length;
            }
        }

        private void MoveCaretToEnd()
        {
            _textEditor.CaretOffset = Doc.TextLength;
            _textEditor.ScrollToEnd(); 
        }

        private void MoveCaretToAfterPrompt()
        {
            var lastLine = Doc.GetLineByNumber(Doc.LineCount);
            var offsetAfterPrompt = lastLine.Offset + _prompt.Length;
            _textEditor.CaretOffset = offsetAfterPrompt;
        }

        private bool IsCaretAtCurrentLine()
        {
            var offsetLine = Doc.GetLocation(Offset).Line;
            return offsetLine == Doc.LineCount;
        }

        private bool IsCaretAfterPrompt()
        {
            var offsetColumn = Doc.GetLocation(Offset).Column;
            return offsetColumn > _prompt.Length;
        }

        private bool IsCaretAtPrompt()
        {
            var offsetColumn = Doc.GetLocation(Offset).Column;
            return offsetColumn-1 == _prompt.Length;
        }

        private bool IsCaretAtWritablePosition() => IsCaretAtCurrentLine() && IsCaretAfterPrompt();

        private bool IsSelectionBeforePrompOffset() => _textEditor.SelectionLength > 0 && _textEditor.SelectionStart < PromptOffset;

        private void SelectCurrentLineOnly()
        {
            var oldSelectionStart = _textEditor.SelectionStart;
            var oldSelectionEnd = oldSelectionStart + _textEditor.SelectionLength;
            _textEditor.SelectionLength = 0;
            _textEditor.SelectionStart = PromptOffset;
            _textEditor.SelectionLength = oldSelectionEnd - PromptOffset;
        }

        private void SelectCurrentLine()
        {
            _textEditor.SelectionStart = PromptOffset;
            _textEditor.SelectionLength = Doc.TextLength - PromptOffset;
        }

        private string GetCurrentLineText()
        {
            var lastLine = Doc.GetLineByNumber(Doc.LineCount);
            var lastLineText = Doc.GetText(lastLine.Offset, lastLine.Length);
            if (lastLineText.Length >= _prompt.Length)
                return lastLineText.Substring(_prompt.Length);
            else
                return lastLineText;
        }
        #endregion

        #region TextEditor Write Text Helpers
        public void ClearLine()
        {
            var lastLine = Doc.GetLineByNumber(Doc.LineCount);
            Doc.Remove(lastLine.Offset, lastLine.Length);
            Doc.Insert(Doc.TextLength, _prompt);
            MoveCaretToEnd();
        }

        public void WritePrompt()
        {
            //see if the last character is a new line, if not inser a new line
            if (!_textEditor.Text.EndsWith(Environment.NewLine))
                WriteLine();
            Write(_prompt, TextType.None);
        }

        public void Write(string format, params object[] arg) => Write(string.Format(format, arg));

        public void Write(string text) => Write(text, TextType.Output);

        public void Write(string text, TextType textType)
        {
            var startOffset = Doc.TextLength;
            Doc.Insert(Doc.TextLength, text);
            MoveCaretToEnd();
            var endOffset = Doc.TextLength;

            if (textType != TextType.None)
            {
                var colorizer = new OffsetColorizer(GetColor(textType)) { StartOffset = startOffset, EndOffset = endOffset };
                _textEditor.TextArea.TextView.LineTransformers.Add(colorizer);
            }
        }

        public void WriteLine() => Write(Environment.NewLine, TextType.None);

        public void WriteLine(string format, params object[] arg) => Write(string.Format(format, arg) + Environment.NewLine);

        public void WriteLine(string text) => Write(text + Environment.NewLine);

        public void WriteLine(string text, TextType textType) => Write(text + Environment.NewLine, textType);

        private string ToPrettyString(object o)
        {
            if (o is string)
                return o.ToString();

            var enumerable = o as IEnumerable;
            if (enumerable != null)
            {
                var items = enumerable.Cast<object>().Take(21).ToList();
                var firstItems = items.Take(20).ToList();
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append(string.Join(", ", firstItems));
                if (items.Count > firstItems.Count)
                    sb.Append("...");
                sb.Append("}");
                return sb.ToString();
            }
            return o.ToString();
        }
        #endregion

        #region IReplOutput interface, colors and fonts
        public Color GetColor(TextType textType)
        {
            switch (textType)
            {
                case TextType.Warning:
                    return WarningColor;
                case TextType.Error:
                    return ErrorColor;
                case TextType.Repl:
                    return TextColor;
                case TextType.Output:
                case TextType.None:
                default:
                    return ResultColor;
            }
        }

        public void ResetColor()
        {
            ResultColor = Color.FromArgb(255, 78, 78, 78);
            WarningColor = Color.FromArgb(255, 183, 122, 0);
            ErrorColor = Color.FromArgb(255, 138, 6, 3);
            TextColor = Color.FromArgb(255, 0, 127, 0);
            BackgroundColor = Colors.WhiteSmoke;
        }

        public string Font
        {
            get { return _textEditor.FontFamily.ToString(); }
            set { _textEditor.FontFamily = new FontFamily(value); }
        }

        public new double FontSize
        {
            get { return _textEditor.FontSize; }
            set { _textEditor.FontSize = value; }
        }

        public Color BackgroundColor
        {
            get
            {
                var b = _textEditor.Background as SolidColorBrush;
                if (b != null) return b.Color;
                else
                    return Colors.Black;
            }
            set { _textEditor.Background = new SolidColorBrush(value); }
        }

        public Color ResultColor { get; set; }
        public Color WarningColor { get; set; }
        public Color ErrorColor { get; set; }
        public Color TextColor { get; set; }
        #endregion

    }//end class
}
