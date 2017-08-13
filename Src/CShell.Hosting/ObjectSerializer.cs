using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace CShell.Hosting
{
    internal class ObjectSerializer : IObjectSerializer
    {
        public string Serialize(object value)
        {
            throw new NotImplementedException();
        }
    }

    class MockConsole : IConsole
    {
        public string ReadLine(string prompt)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public ConsoleColor ForegroundColor
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public int Width { get; }

        public string ReadLine()
        {
            throw new NotImplementedException();
        }

        public void ResetColor()
        {
            throw new NotImplementedException();
        }

        public void Write(string value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(string value)
        {
            throw new NotImplementedException();
        }

        public void WriteLine()
        {
            throw new NotImplementedException();
        }
    }
}
