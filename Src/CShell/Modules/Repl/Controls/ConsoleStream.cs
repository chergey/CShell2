using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace CShell.Modules.Repl.Controls
{
    internal class ConsoleStream : Stream
    {
        private readonly TextType _textType;
        readonly Action<string, TextType> _callback;

        public ConsoleStream(TextType textType, Action<string, TextType> cb)
        {
            this._textType = textType;
            _callback = cb;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;


        public override long Length => 0;
        public override long Position { get { return 0; } set { } }
        public override void Flush() { }
        public override int Read([In, Out] byte[] buffer, int offset, int count) => -1;

        public override long Seek(long offset, SeekOrigin origin) => 0;

        public override void SetLength(long value) { }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Execute.OnUIThread(() => _callback(Encoding.UTF8.GetString(buffer, offset, count), _textType));
        }
    }
}
