using System.Collections.Generic;

namespace Lox.Lexer
{
    /// <summary>
    /// The first step in any compiler or interpreter is scanning.
    /// The scanner takes in raw source code as a series of characters
    /// and groups it into a series of chunks we call tokens.
    /// </summary>
    public class Scanner
    {
        /// <summary>
        /// The dictionary holds the reserved words so that we can check
        /// if the identifier’s lexeme is one of the reserved words
        /// </summary>
        private static readonly Dictionary<string, TokenType> _reservedKeywords = new();
        /// <summary>
        /// The raw source code that is read
        /// </summary>
        private readonly string _source;
        /// <summary>
        /// a list to be filled with tokens that will be generated in scanning phase
        /// </summary>
        private readonly List<Token> _tokens = new();
        /// <summary>
        /// _start field points to the first character in the lexeme being scanned
        /// </summary>
        private int _start = 0;
        /// <summary>
        /// _current field points at the character currently being considered
        /// </summary>
        private int _current = 0;
        /// <summary>
        /// The line field tracks what source line current is on so we can produce tokens that know their location
        /// </summary>
        private int _line = 1;

        /// <summary>
        /// Initialization of dictionary data so that we can use them before instatiating any object
        /// </summary>
        static Scanner()
        {
            _reservedKeywords["and"] = TokenType.AND;
            _reservedKeywords["class"] = TokenType.CLASS;
            _reservedKeywords["else"] = TokenType.ELSE;
            _reservedKeywords["false"] = TokenType.FALSE;
            _reservedKeywords["true"] = TokenType.TRUE;
            _reservedKeywords["for"] = TokenType.FOR;
            _reservedKeywords["while"] = TokenType.WHILE;
            _reservedKeywords["fun"] = TokenType.FUN;
            _reservedKeywords["null"] = TokenType.NULL;
            _reservedKeywords["or"] = TokenType.OR;
            _reservedKeywords["print"] = TokenType.PRINT;
            _reservedKeywords["return"] = TokenType.RETURN;
            _reservedKeywords["super"] = TokenType.SUPER;
            _reservedKeywords["this"] = TokenType.THIS;
            _reservedKeywords["var"] = TokenType.VAR;
        }

        /// <summary>
        /// Takes source code to initialize _source field so that start processing of scanning phase. 
        /// </summary>
        /// <param name="source"></param>
        public Scanner(string source)
        {
            _source = source;
        }



        /// <summary>
        /// he scanner works its way through the source code, adding tokens until it runs out of characters.
        /// Then it appends one final “end of file” token.
        /// </summary>
        /// <returns></returns>
        public List<Token> ScanTokens()
        {
            // Check if we reached the end of source code or not
            while (!IsAtEnd())
            {
                // The beginning of the next lexeme.
                _start = _current;
                ScanToken();
            }
            //Reached the end of the file, so we put EOF token.
            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;

        }

        /// <summary>
        /// In each turn of the loop, we scan a single token.
        /// consuming the next character and pick a token type for it
        /// </summary>
        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!': AddToken( MatchNextChar('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken( MatchNextChar('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken( MatchNextChar('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken( MatchNextChar('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/':
                    if (MatchNextChar('/')) SkipSingleLineComment();
                    else if (MatchNextChar('*')) SkipMultiLineComments();
                    else AddToken(TokenType.SLASH);

                    break;

                // Ignore whitespaces
                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n': _line++; break;
                case '"': AddStringToken(); break;

                default:
                    if (char.IsDigit(c)) AddNumberToken();
                    else if (char.IsLetter(c)) AddIdentifierToken();
                    //if a user throws a source file containing some characters Lox doesn’t use,like @#^,
                    //those characters get silently discarded but a report of error will be generated
                    else Lox.Error(_line, "Unexpected character.");
                    break;
            }
        }

        /// <summary>
        /// Scanning actual string value so that it can be added as a token to be further processed.
        /// </summary>
        private void AddStringToken()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                _current++;
            }

            if (IsAtEnd())
            {
                Lox.Error(_line, "Syntax Error: Unterminated string");
                return;
            }

            // The closing ".
               _current++;


            string textValue = _source.Substring(_start + 1, _current - 1);

            AddToken( TokenType.STRING, textValue);

        }

        /// <summary>
        /// Scanning actual number values so that it can be added as a token to be further processed.
        /// </summary>
        private void AddNumberToken()
        {
            while (char.IsDigit(Peek())) _current++;

            if (Peek() == '.' && char.IsDigit(PeekNext())) _current++;
            

            while (char.IsDigit(Peek())) _current++;

            AddToken(TokenType.NUMBER, double.Parse(_source.Substring(_start,_current)));

        }

        /// <summary>
        /// Scanning variables (identifiers) or reserved keywords
        /// </summary>
        private void AddIdentifierToken()
        {
            while (char.IsLetterOrDigit(Peek())) _current++;

            string text = _source.Substring(_start, _current);

            if (_reservedKeywords.TryGetValue(text, out TokenType type)) AddToken(type);
            else AddToken(TokenType.IDENTIFIER);


        }

        /// <summary>
        /// The method responsible for checking if we reached the end of source code or not.
        /// </summary>
        /// <returns></returns>
        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        /// <summary>
        /// It takes the current character in the source file and returns it,
        /// then advancing to the next character
        /// </summary>
        /// <returns></returns>
        private char Advance()
        {
            return _source[_current++];
        }
   
        /// <summary>
        /// The method responsible for adding tokens to the _tokenList
        /// </summary>
        /// <param name="type">Token type</param>
        /// <param name="literal">
        /// The actual text representation: string, numbers,
        /// otherwise it will be null as no real value is concerned.
        /// </param>
        private void AddToken(TokenType type, object? literal = null)
        {
            string text = _source.Substring(_start, _current);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        /// <summary>
        /// check if the matching character is true or false
        /// </summary>
        /// <param name="expectedChar">The character to be check agains</param>
        /// <returns>boolean value indicating the character is expected or not</returns>
        private bool MatchNextChar (char expectedChar)
        {
            if (IsAtEnd() || _source[_current] != expectedChar) return false;

            _current++;

            return true;
        }

        /// <summary>
        /// return the current character of the scanning process of source code
        /// </summary>
        /// <returns>The current character</returns>
        private char Peek()
        {
            //if (IsAtEnd()) return '\0';
            return _source[_current];
        }


        /// <summary>
        /// return the next character of the scanning process of source code
        /// </summary>
        /// <returns>The next character</returns>
        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        /// <summary>
        /// It skips a single line comments so that it will be discarded
        /// </summary>
        private void SkipSingleLineComment ()
        {
            // A comment goes until the end of the line.
            while (!IsAtEnd() && Peek() != '\n') _current++;
            //inreace number of lines so that we can check the location of errors accuretely.
            if (Peek() == '\n') _line++;
        }

        /// <summary>
        /// Skip multline comments of being processed by Scanner 
        /// </summary>
        private void SkipMultiLineComments()
        {
            /*,,*/
            while (!IsAtEnd() && Peek() != '*' && PeekNext() != '/' ) {
                if (Peek() == '\n') _line++;
                _current++;
            }
        }

    }
}
