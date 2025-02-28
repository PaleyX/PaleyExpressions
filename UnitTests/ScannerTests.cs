using static PaleyExpressions.TokenType;

namespace UnitTests
{
    public class ScannerTests
    {
        [Fact]
        public void Scanner_Tokenises_Correctly()
        {
            var scanner = new PaleyExpressions.Scanner("1+true+gort");

            var tokens = scanner.ScanTokens();

            Assert.Equal(5 + 1, tokens.Count);

            Assert.Equal(NUMBER, tokens[0].TokenType);
            Assert.Equal("1", tokens[0].Lexeme);
            Assert.Equal(1d, tokens[0].Literal);

            Assert.Equal(PLUS, tokens[1].TokenType);
            Assert.Equal("+", tokens[1].Lexeme);
            Assert.Null(tokens[1].Literal);

            Assert.Equal(TRUE, tokens[2].TokenType);
            Assert.Equal("true", tokens[2].Lexeme);
            Assert.Null(tokens[2].Literal);

            Assert.Equal(PLUS, tokens[3].TokenType);
            Assert.Equal("+", tokens[3].Lexeme);
            Assert.Null(tokens[3].Literal);

            Assert.Equal(IDENTIFIER, tokens[4].TokenType);
            Assert.Equal("gort", tokens[4].Lexeme);
            Assert.Null(tokens[4].Literal);

            Assert.Equal(EOF, tokens[5].TokenType);
        }
    }
}
