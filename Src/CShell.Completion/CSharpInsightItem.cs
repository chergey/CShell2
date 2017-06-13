using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CShell.Completion.DataItems;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace CShell.Completion
{
    public sealed class CSharpInsightItem
    {
        public readonly IParameterizedMember Method;

        public CSharpInsightItem(IParameterizedMember method)
        {
            this.Method = method;
        }

        TextBlock _header;

        public object Header
        {
            get
            {
                if (_header == null)
                {
                    _header = new TextBlock();
                    GenerateHeader();
                }
                return _header;
            }
        }

        int _highlightedParameterIndex = -1;

        public void HighlightParameter(int parameterIndex)
        { 
            if (_highlightedParameterIndex == parameterIndex)
                return;
            this._highlightedParameterIndex = parameterIndex;
            if (_header != null)
                GenerateHeader();
        }

        void GenerateHeader()
        {
            CSharpAmbience ambience = new CSharpAmbience();
            ambience.ConversionFlags = ConversionFlags.StandardConversionFlags;
            var stringBuilder = new StringBuilder();
            var formatter = new ParameterHighlightingOutputFormatter(stringBuilder, _highlightedParameterIndex);
            ambience.ConvertEntity(Method, formatter, FormattingOptionsFactory.CreateSharpDevelop());
            var inlineBuilder = new HighlightedInlineBuilder(stringBuilder.ToString());
            inlineBuilder.SetFontWeight(formatter.ParameterStartOffset, formatter.ParameterLength, FontWeights.Bold);
            _header.Inlines.Clear();
            _header.Inlines.AddRange(inlineBuilder.CreateRuns());
        }

        public object Content => Documentation;

        private string _documentation;
        public string Documentation
        {
            get
            {
                if (_documentation == null)
                {
                    if (Method.Documentation == null)
                        _documentation = "";
                    else
                        _documentation = EntityCompletionData.XmlDocumentationToText(Method.Documentation);
                }
                return _documentation;
            }
        }

        sealed class ParameterHighlightingOutputFormatter : TextWriterTokenWriter
        {
            StringBuilder _b;
            int _highlightedParameterIndex;
            int _parameterIndex;
            internal int ParameterStartOffset;
            internal int ParameterLength;

            public ParameterHighlightingOutputFormatter(StringBuilder b, int highlightedParameterIndex)
                : base(new StringWriter(b))
            {
                this._b = b;
                this._highlightedParameterIndex = highlightedParameterIndex;
            }

            public override void StartNode(AstNode node)
            {
                if (_parameterIndex == _highlightedParameterIndex && node is ParameterDeclaration)
                {
                    ParameterStartOffset = _b.Length;
                }
                base.StartNode(node);
            }

            public override void EndNode(AstNode node)
            {
                base.EndNode(node);
                if (node is ParameterDeclaration)
                {
                    if (_parameterIndex == _highlightedParameterIndex)
                        ParameterLength = _b.Length - ParameterStartOffset;
                    _parameterIndex++;
                }
            }
        }
    }
}
