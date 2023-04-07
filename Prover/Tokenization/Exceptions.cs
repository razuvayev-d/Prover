using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prover.Tokenization
{

    internal class LexerException : Exception
    {
        public int Position { get; }

        public override string Message => base.Message + string.Format(" на позиции {0}.", Position);

        public LexerException() { }

        public LexerException(string message, int position) : base(message)
        {
            this.Position = position;
        }



        public LexerException(string message, Exception innerException) : base(message, innerException) { }
    }
    internal class UnexpectedTokenException : LexerException
    {
        public TokenType[] ExpectedTokens { get; }
        public TokenType Unexpected { get; }
        public override string Message
        {
            get
            {
                return ExpectedTokens is null ? base.Message : base.Message + string.Format(" Встречено \"{0}\", а ожидалось что-нибудь из этого: {1}.",
                    Lexer.FromTokenType(Unexpected), Lexer.TokenList(ExpectedTokens));
            }
        }
        public UnexpectedTokenException()
        {
        }

        public UnexpectedTokenException(string message, int position, TokenType[] expected, TokenType unexpected)
            : base(message, position)
        {
            ExpectedTokens = expected;
            Unexpected = unexpected;
        }

        public UnexpectedTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    internal class UnexpectedIdentifierException : LexerException
    {
        public string Identifier { get; }
        public string[] Expected { get; }

        public override string Message
        {
            get
            {
                return Expected is null ? base.Message : base.Message + string.Format(" Встречено \"{0}\", а ожидалось что-нибудь из этого: {1}.",
                    Identifier, ToStr(Expected));
            }
        }

        public UnexpectedIdentifierException(string message, int position, string[] expected, string unexpected) : base(message, position) 
        {
            Identifier = unexpected;
            Expected = expected;
        }

        private static string ToStr(string[] list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in list)
            {
                sb.Append(string.Format("\"{0}\" ", item));
            }
            return sb.ToString();
        }
    }
}
