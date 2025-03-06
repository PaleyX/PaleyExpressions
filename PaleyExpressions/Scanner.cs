using static PaleyExpressions.TokenType;

namespace PaleyExpressions
{
    internal class Scanner(string source)
    {
        private readonly List<Token> _tokens = [];
        private int _start;
        private int _current;

        private static readonly Dictionary<string, TokenType> _keywords = new()
        {
            { "and",    AND },
            { "false",  FALSE },
            { "nil",    NIL },
            { "or",     OR },
            { "true",   TRUE },
        };

        internal List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(EOF, "", null));

            return _tokens;
        }

        private bool IsAtEnd()
        {
            return _current >= source.Length;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case ',': AddToken(COMMA); break;
                case '.': AddToken(DOT); break;
                case '-': AddToken(MINUS); break;
                case '+': AddToken(PLUS); break;
                case ';': AddToken(SEMICOLON); break;
                case '*': AddToken(STAR); break;
                case '/': AddToken(SLASH); break;
                case '!':
                    AddToken(Match('=') ? BANG_EQUAL : BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? LESS_EQUAL : LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;
                case '"': ScanString(); break;
                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        throw new ScannerException($"Unexpected character: '{c}'");
                    }
                    break;
            }
        }

        private void ScanString () 
        {
            while (Peek() != '"' && !IsAtEnd()) 
            {
                Advance();
            }

            if (IsAtEnd())
            {
                throw new ScannerException("Unterminated string.");
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            var value = source[(_start + 1)..(_current - 1)];
            AddToken(STRING, value);
        }

        private void ScanNumber()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(NUMBER, double.Parse(source[_start.._current]));
        }

        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            var text = source[_start.._current];

            var tokenType = _keywords.GetValueOrDefault(text, IDENTIFIER);

            AddToken(tokenType);
        }

        private bool Match(char expected)
        {
            if (IsAtEnd())
            {
                return false;
            }

            if (source[_current] != expected)
            {
                return false;
            }

            _current++;

            return true;
        }

        private char Advance()
        {
            return source[_current++];
        }

        private char Peek()
        {
            if (IsAtEnd())
            {
                return '\0';
            }

            return source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= source.Length)
            {
                return '\0';
            }

            return source[_current + 1];
        }

        private void AddToken(TokenType type, object? literal = null)
        {
            var text = source[_start.._current];
            _tokens.Add(new Token(type, text, literal));
        }

        private static bool IsDigit(char c) => c is >= '0' and <= '9';

        private static bool IsAlpha(char c)
        {
            return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
        }

        private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
    }
}
