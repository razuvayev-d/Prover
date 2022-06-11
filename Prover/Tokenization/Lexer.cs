using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;

namespace Prover.Tokenization
{

    public enum TokenType
    {
        NoToken,
        WhiteSpace,
        Comment,
        IdentUpper,
        IdentLower,
        DefFunctor,
        Integer,
        FullStop,
        OpenPar,
        ClosePar,
        OpenSquare,
        CloseSquare,
        Comma,
        Colon,
        EqualSign,
        NotEqualSign,
        Nand,
        Nor,
        Or,
        And,
        Implies,
        BImplies,
        Equiv,
        Xor,
        Universal,
        Existential,
        Negation,
        SQString,
        EOFToken
    }

    public class Token
    {
        public TokenType type;
        public string literal;
        string source;
        int pos;

        public Token(TokenType type, string literal, string source, int pos)
        {
            this.type = type;
            this.literal = literal;
            this.source = source;
            this.pos = pos;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\" ({1})", literal, type.ToString());
        }
    }

    /// <summary>
    /// Лексический анализатор
    /// </summary>
    public class Lexer
    {
        static List<(Regex, TokenType)> tokenDefs = new List<(Regex, TokenType)>{
            (new Regex(@"^\."),                    TokenType.FullStop),
            (new Regex(@"^\("),                    TokenType.OpenPar),
            (new Regex(@"^\)"),                    TokenType.ClosePar),
            (new Regex(@"^\["),                    TokenType.OpenSquare),
            (new Regex(@"^\]"),                    TokenType.CloseSquare),
            (new Regex(@"^,"),                     TokenType.Comma),
            (new Regex(@"^:"),                     TokenType.Colon),
            (new Regex(@"^~\|"),                   TokenType.Nor),
            (new Regex(@"^~&"),                    TokenType.Nand),
            (new Regex(@"^\|"),                    TokenType.Or),
            (new Regex(@"^&"),                     TokenType.And),
            (new Regex(@"^=>"),                    TokenType.Implies),
            (new Regex(@"^<=>"),                   TokenType.Equiv),
            (new Regex(@"^<="),                    TokenType.BImplies),
            (new Regex(@"^<~>"),                   TokenType.Xor),
            (new Regex(@"^="),                     TokenType.EqualSign),
            (new Regex(@"^!="),                    TokenType.NotEqualSign),
            (new Regex(@"^~"),                     TokenType.Negation),
            (new Regex(@"^!"),                     TokenType.Universal),
            (new Regex(@"^\?"),                    TokenType.Existential),
            (new Regex(@"^\s+"),                   TokenType.WhiteSpace),
            (new Regex(@"^[0-9][0-9]*"),           TokenType.IdentLower),
            (new Regex(@"^[a-z][_a-z0-9_A-Z]*"),   TokenType.IdentLower),
            (new Regex(@"^[_A-Z][_a-z0-9_A-Z]*"),  TokenType.IdentUpper),
            (new Regex(@"^\$[_a-z0-9_A-Z]*"),      TokenType.DefFunctor),
            (new Regex(@"^#[^\n]*"),               TokenType.Comment),
            (new Regex(@"^%[^\n]*"),               TokenType.Comment),
            (new Regex(@"^'[^']*'"),               TokenType.SQString)
        };

        string source;
        string name;
        int pos;
        Stack<Token> tokenStack;
        public int Pos => pos;
        public Lexer(string source, string name = "user string")
        {
            this.source = source;
            this.name = name;
            pos = 0;
            tokenStack = new Stack<Token>();
        }

        void Push(Token token)
        {
            tokenStack.Push(token);
        }



        /// <summary>
        /// Возвращает следующий семантически подходящий токен
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Token Next()
        {
            var res = NextUnfiltered();
            while (res.type == TokenType.WhiteSpace || res.type == TokenType.Comment)
                res = NextUnfiltered();
            return res;
        }

        /// <summary>
        /// Возвращает следующую лексему, включая лексемы, игнорируемые большинством языков.
        /// </summary>
        /// <returns></returns>
        Token NextUnfiltered()
        {
            if (tokenStack.Count > 0)
                return tokenStack.Pop();

            int old_pos = pos;

            if (source.Substring(old_pos) == "")
                return new Token(TokenType.EOFToken, string.Empty, source, old_pos);
            foreach (var i in tokenDefs)
            {
                var mr = i.Item1.Match(source.Substring(pos)/*, pos*/); //Заменить на source.Substring(pos)
                if (mr.Success)
                {
                    var literal = source.Substring(old_pos/*mr.Index*/, mr.Length);
                    pos = pos + mr.Length; // mr.Index + mr.Length;
                    var type = i.Item2;
                    return new Token(type, literal, source, old_pos);
                }
            }
            throw new ArgumentException("IllegalCharacterError (not impl)");
        }
        Token Look()
        {
            var res = Next();
            Push(res);
            return res;
        }

        /// <summary>
        /// Возвращает буквальное значение следующего маркера, т.е.строку
        /// порождающая маркер.
        /// </summary>
        public string LookLit() => Look().literal;

        /// <summary>
        /// Принимает список ожидающихся типов токенов. True если следующий токен ожидается, false в противном случае
        /// </summary>
        /// <param name=""></param>
        public bool TestTok(params TokenType[] tokenTypes)
        {
            return tokenTypes.Contains(Look().type);
        }

        /// <summary>
        /// Взять список ожидаемых типов лексем. Если следующий токен
        ///не входит в число ожидаемых, выйдите с ошибкой.В противном случае
        ///ничего.
        /// </summary>
        /// <param name="tokenTypes"></param>
        public void CheckTok(params TokenType[] tokenTypes)
        {
            if (!TestTok(tokenTypes)) throw new ArgumentException("UnexpectedTokenError (not imp). " + source[pos]);

        }

        /// <summary>
        /// 
        ///Возьмите список ожидаемых типов лексем.Если следующий токен
        ///      среди ожидаемых, потребляем и возвращаем его.
        ///      В противном случае выйти с ошибкой.
        /// </summary>
        /// <param name="tokenTypes"></param>
        /// <returns></returns>
        public Token AcceptTok(params TokenType[] tokenTypes)
        {
            CheckTok(tokenTypes);
            return Next();
        }

        public bool TestLit(params string[] litvals)
        {
            return litvals.Contains(LookLit());
        }

        public void CheckLit(params string[] litvals)
        {
            if (!TestLit(litvals))
                throw new ArgumentException("UnexpectedIdentError (ni)");

        }

        public Token AcceptLit(params string[] litvals)
        {
            CheckLit(litvals);
            return Next();
        }


        /// <summary>
        /// Возвращает список всех токенов из источника
        /// </summary>
        /// <returns></returns>
        //List<Token> Lex()
        //{
        //    var res = new List<Token>();
        //    while (!TestTok(TokenType.EOFToken))
        //        res.Add(Next());
        //    return res;
        //}
    }
}
