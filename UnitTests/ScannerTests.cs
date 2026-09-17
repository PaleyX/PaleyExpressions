using PaleyExpressions;
using static PaleyExpressions.TokenType;

namespace UnitTests;

public class ScannerTests
{
    [Fact]
    public void Scanner_Tokenises_Correctly()
    {
        var scanner = new Scanner("1+true+gort");
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

    [Theory]
    [InlineData("10", 10d)]
    [InlineData("378.8765", 378.8765d)]
    [InlineData("0xFF", 0xFF)]
    [InlineData("0x1234", 0x1234)]
    [InlineData("0xf", 0xf)]
    public void NumbersScanCorrectly(string expression, double expected)
    {
        var scanner = new Scanner(expression);
        var tokens = scanner.ScanTokens();

        Assert.Equal(2, tokens.Count);
        Assert.Equal(NUMBER, tokens[0].TokenType);
        Assert.Equal(EOF, tokens[1].TokenType);
        Assert.IsType<double>(tokens[0].Literal);
        Assert.Equal(expected, tokens[0].Literal);
    }

    [Theory]
    [InlineData("0xFF.87", "Unexpected character: '.'")]
    [InlineData("10..9", "Unexpected character: '.'")]
    [InlineData("0xx", "Empty hexadecimal number")]
    public void BadNumbersThrowCorrectly(string expression, string message)
    {
        var scanner = new Scanner(expression);

        var ex = Assert.Throws<ExpressionException>(scanner.ScanTokens);
        Assert.Equal(message, ex.Message);
    }
}