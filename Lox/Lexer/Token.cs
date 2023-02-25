namespace Lox.Lexer
{
    public class Token
    {
        private readonly TokenType _tokenType;
        /// <summary>
        /// The lexemes are only the raw substrings of the source code such as: "while", "(", "a", "=", "1". "" ";"
        /// </summary>
        private readonly string _lexeme;
        private readonly object? _literal;
        private readonly int _line;

        public Token(TokenType tokenType, string lexeme, object? literal, int line)
        {
            _tokenType = tokenType;
            _lexeme = lexeme;
            _literal = literal;
            _line = line;
        }

        public override string ToString()
        {
            return _tokenType + " " + _lexeme + " " + _literal;
        }
    }
}
