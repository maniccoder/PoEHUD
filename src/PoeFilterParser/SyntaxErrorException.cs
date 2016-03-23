using System;

namespace PoeFilterParser
{
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(int line, int charPositionInLine, string message) : base(message)
        {
            Line = line;
            CharPositionInLine = charPositionInLine;
        }

        public int Line { get; }
        public int CharPositionInLine { get; }
    }
}