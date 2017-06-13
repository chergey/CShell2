using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.NRefactory.CSharp;

namespace CShell.Completion
{
    /// <summary>
    /// Output formatter that creates a dictionary from AST nodes to segments in the output text.
    /// </summary>
    public class SegmentTrackingOutputFormatter : TextWriterTokenWriter
    {
        Dictionary<AstNode, ICSharpCode.AvalonEdit.Document.ISegment> _segments = new Dictionary<AstNode, ICSharpCode.AvalonEdit.Document.ISegment>();
        Stack<int> _startOffsets = new Stack<int>();
        readonly StringWriter _stringWriter;

        public IDictionary<AstNode, ICSharpCode.AvalonEdit.Document.ISegment> Segments => _segments;

        public SegmentTrackingOutputFormatter(StringWriter stringWriter)
            : base(stringWriter)
        {
            this._stringWriter = stringWriter;
        }

        public static IDictionary<AstNode, ICSharpCode.AvalonEdit.Document.ISegment> WriteNode(StringWriter writer, AstNode node, CSharpFormattingOptions policy, ICSharpCode.AvalonEdit.TextEditorOptions options)
        {
            var formatter = new SegmentTrackingOutputFormatter(writer);
            formatter.IndentationString = options.IndentationString;
            var visitor = new CSharpOutputVisitor(formatter, policy);
            node.AcceptVisitor(visitor);
            return formatter.Segments;
        }

        public override void StartNode(AstNode node)
        {
            base.StartNode(node);
            _startOffsets.Push(_stringWriter.GetStringBuilder().Length);
        }

        public override void EndNode(AstNode node)
        {
            int startOffset = _startOffsets.Pop();
            StringBuilder b = _stringWriter.GetStringBuilder();
            int endOffset = b.Length;
            while (endOffset > 0 && b[endOffset - 1] == '\r' || b[endOffset - 1] == '\n')
                endOffset--;
            _segments.Add(node, new TextSegment { StartOffset = startOffset, EndOffset = endOffset });
            base.EndNode(node);
        }
    }
}
