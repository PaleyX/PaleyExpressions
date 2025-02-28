using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaleyExpressions
{
    internal class Token
    {
        private readonly TokenType _type;
        private readonly string _lexeme;
        private readonly object? _literal;

        internal Token(TokenType type, string lexeme, object? literal)
        {
            _type = type;
            _lexeme = lexeme;
            _literal = literal;
        }

        public override string ToString()
        {
            return $"{_type} {_lexeme} {_literal}";
        }
    }
}
