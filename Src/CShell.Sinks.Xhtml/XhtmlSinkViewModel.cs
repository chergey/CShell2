using System;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CShell.Framework.Services;
using CShell.Sinks.Xhtml.XhtmlDumper;

namespace CShell.Sinks.Xhtml
{
    public class XhtmlSinkViewModel : Framework.Sink
    {
        private StringBuilder _stringBuilder;
        private StringWriter _stringWriter;
        private XhtmlDumper.XhtmlDumper _xhtmlDumper;

        private TextWriter _linqPadWriter;
        
        public XhtmlSinkViewModel(Uri uri)
        {
            Uri = uri;
            DisplayName = GetTitle(uri, "Dump");
        }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        private string _text = "";
        public string Text => _text;

        public override void Dump(object o)
        {
            if (_xhtmlDumper == null)
            {
                _stringBuilder = new StringBuilder();
                _stringWriter = new StringWriter(_stringBuilder);
                var renderers = IoC.GetAllInstances(typeof(IXhtmlRenderer)).Cast<IXhtmlRenderer>().ToList();
                //add the basic rederer at the end
                renderers.Add(new BasicXhtmlRenderer());
                _xhtmlDumper = new XhtmlDumper.XhtmlDumper(_stringWriter);
            }

            _xhtmlDumper.WriteObject(o, null, 3);
            _text = _stringBuilder.ToString();
            //append the closing HTML closing tags to the string
            _text += Environment.NewLine + "</body></html>";

            NotifyOfPropertyChange(()=>Text);
        }

        public override void Clear()
        {
            if(_linqPadWriter != null)
                _linqPadWriter.Dispose();
            _linqPadWriter = null;

            _text = string.Empty;
            NotifyOfPropertyChange(() => Text);
        }

        protected override void OnDeactivate(bool close)
        {
            Clear();
            base.OnDeactivate(close);
        }

        public override bool Equals(object obj)
        {
            var other = obj as XhtmlSinkViewModel;
            return other != null && Uri == other.Uri;
        }

    }
}
