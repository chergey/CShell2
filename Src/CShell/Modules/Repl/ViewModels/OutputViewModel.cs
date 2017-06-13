using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Windows.Media;
using CShell.Framework;
using CShell.Framework.Services;
using CShell.Modules.Repl.Views;
using Caliburn.Micro;
using Execute = CShell.Framework.Services.Execute;

namespace CShell.Modules.Repl.ViewModels
{
    [Export]
    [Export(typeof(IOutput))]
    [Export(typeof(ITool))]
    public class OutputViewModel : Tool, IOutput
	{
		private IOutputView _view;

        [ImportingConstructor]
        public OutputViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
            BufferLength = 200;
            Font = "Consolas";
            FontSize = 12;
            BackgroundColor = Color.FromArgb(255, 40, 40, 40);
            TextColor = Color.FromArgb(255, 242, 242, 242);
            DisplayName = "Output";
        }

		public override PaneLocation PreferredLocation => PaneLocation.Bottom;

        public override Uri Uri => new Uri("tool://cshell/output");

        public override Uri IconSource => new Uri("pack://application:,,,/CShell;component/Resources/Icons/Output.png");

        private string _text = string.Empty;
		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
                if (_text != null)
                {
                    var lines = _text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    //only remove old lines every 10 updates 
                    if (lines.Length > BufferLength && (_bufferLength <= 10 || lines.Length % 10 == 0))
                        _text = string.Join(Environment.NewLine, lines.Skip(lines.Length - _bufferLength));
                }
			    NotifyOfPropertyChange(() => Text);

				if (_view != null)
					Execute.OnUiThread(() => _view.ScrollToEnd());
			}
		}

	    private int _bufferLength;
	    public int BufferLength
	    {
	        get { return _bufferLength; }
	        set
	        {
                if (value <= 0)
                    throw new ArgumentException("BufferLength has to be 1 or greater");
                _bufferLength = value;
	        }
	    }

	    public void Clear()
		{
			Text = string.Empty;
		}

        public void Write(string text)
        {
            Text += text;
        }

        public void WriteLine()
        {
            Text += Environment.NewLine;
        }

        public void WriteLine(string text)
        {
            Text += text + Environment.NewLine;
        }

        public void Write(string format, params object[] arg)
        {
            Text += string.Format(format, arg);
        }

        public void WriteLine(string format, params object[] arg)
        {
            Text += string.Format(format, arg) + Environment.NewLine;
        }

		protected override void OnViewLoaded(object view)
		{
			_view = (IOutputView) view;
			_view.ScrollToEnd();
		}

        #region Appearance from IOutput
        private string _font;
        public string Font
        {
            get { return _font; }
            set { _font = value; NotifyOfPropertyChange(() => Font); }
        }

        private double _fontSize;
        public double FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; NotifyOfPropertyChange(() => FontSize); }
        }

        private Color _textColor;
        public Color TextColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                NotifyOfPropertyChange(() => TextColor);
                NotifyOfPropertyChange(() => TextBrush);
            }
        }
        public Brush TextBrush => new SolidColorBrush(TextColor);

        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                NotifyOfPropertyChange(() => BackgroundColor);
                NotifyOfPropertyChange(() => BackgroundBrush);

            }
        }
        public Brush BackgroundBrush => new SolidColorBrush(BackgroundColor);

        #endregion

        
    }
}