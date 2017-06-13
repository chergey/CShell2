using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using CShell.Sinks.Xhtml.Properties;

namespace CShell.Sinks.Xhtml.XhtmlDumper
{
    public class XhtmlDumper : IDisposable
    {
        private readonly XhtmlTextWriter _writer;
        private readonly IXhtmlRenderer[] _renderers;

        public XhtmlDumper(TextWriter writer)
        {
            this._writer = new XhtmlTextWriter(writer); ;
            this._renderers = new IXhtmlRenderer[] { new ObjectXhtmlRenderer(), new BasicXhtmlRenderer() };
            InitHeader();
        }

        public XhtmlDumper(TextWriter writer, params IXhtmlRenderer[] renderers)
        {
            this._writer = new XhtmlTextWriter(writer);
            this._renderers = renderers;
            InitHeader();
        }

        private void InitHeader()
        {
            _writer.WriteLineNoTabs("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">");
            _writer.AddAttribute("xmlns", "http://www.w3.org/1999/xhtml");
            _writer.AddAttribute("xml:lang", "en");
            _writer.RenderBeginTag(HtmlTextWriterTag.Html);
            _writer.RenderBeginTag(HtmlTextWriterTag.Head);

            _writer.RenderBeginTag(HtmlTextWriterTag.Title);
            _writer.Write("XhtmlDumper");
            _writer.RenderEndTag();


            _writer.AddAttribute("http-equiv", "content-type");
            _writer.AddAttribute(HtmlTextWriterAttribute.Content, "text/html;charset=utf-8");
            _writer.RenderBeginTag(HtmlTextWriterTag.Meta);
            _writer.RenderEndTag();
            _writer.WriteLine();

            _writer.AddAttribute(HtmlTextWriterAttribute.Name, "generator");
            _writer.AddAttribute(HtmlTextWriterAttribute.Content, "XhtmlDumper");
            _writer.RenderBeginTag(HtmlTextWriterTag.Meta);
            _writer.RenderEndTag();
            _writer.WriteLine();

            _writer.AddAttribute(HtmlTextWriterAttribute.Name, "description");
            _writer.AddAttribute(HtmlTextWriterAttribute.Content, "Generated on: " + DateTime.Now);
            _writer.RenderBeginTag(HtmlTextWriterTag.Meta);
            _writer.RenderEndTag();
            _writer.WriteLine();

            _writer.AddAttribute("type", "text/css");
            _writer.RenderBeginTag(HtmlTextWriterTag.Style);
            _writer.WriteLineNoTabs(Resources.StyleSheet);
            _writer.RenderEndTag(); // style

            _writer.RenderEndTag(); // Head
            _writer.WriteLine();

            _writer.RenderBeginTag(HtmlTextWriterTag.Body);
        }


        public void WriteObject(object o, string description, int depth)
        {
            //try to loop through all renderes to see if one will render the object,
            // otherwise fallback on textwriter
            if(!_renderers.Any(xhtmlRenderer => xhtmlRenderer.Render(o, description, depth, _writer)))
                _writer.Write(o);
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer.RenderEndTag(); // body
                _writer.RenderEndTag(); // html
                foreach (var xhtmlRenderer in _renderers)
                {
                    var disposableXhtmlRenderer = xhtmlRenderer as IDisposable;
                    if (disposableXhtmlRenderer != null)
                        disposableXhtmlRenderer.Dispose();
                }
            }
        }

    }
}
