using PaleyExpressions;
using static PaleyExpressions.TokenType;

namespace UnitTests
{
    public class TokenTests
    {
        [Fact]
        public void Token_Properties_AreSet()
        {
            var token = new Token(NUMBER, "123", 123d);

            Assert.Equal(NUMBER, token.TokenType);
            Assert.Equal("123", token.Lexeme);
            Assert.Equal(123d, token.Literal);
        }

        //TODO: Add tests for other token types, e.g., STRING, PLUS, etc.
        [Fact]
        public void ToString_ReturnsExpectedFormat()
        {
            var token = new Token(IDENTIFIER, "gort", null);

            // Matches Token.ToString(): $"{TokenType} {Lexeme} {Literal}"
            Assert.Equal("IDENTIFIER gort ", token.ToString());
        }
    }
}