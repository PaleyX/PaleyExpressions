using PaleyExpressions;

namespace UnitTests;

public class ParserTests
{
    [Theory]
    [InlineData("1!", "!")]
    public void UnexpectedTokenThrowsCorrectly(string expression, string unexpected)
    {
        var tokens = new Scanner(expression).ScanTokens();
        var parser = new Parser(tokens);

        var ex = Assert.Throws<ExpressionException>(() => parser.Parse());

        Assert.Equal($"Unexpected token: at '{unexpected}'", ex.Message);
    }

    [Theory]
    [InlineData("(,)", ",")]
    public void ExpectExpressionThrowsCorrectly(string expression, string unexpected)
    {
        var tokens = new Scanner(expression).ScanTokens();
        var parser = new Parser(tokens);

        var ex = Assert.Throws<ExpressionException>(() => parser.Parse());

        Assert.Equal($"Expect expression: at '{unexpected}'", ex.Message);
    }

    [Theory]
    [InlineData("true and")]
    [InlineData("abs(-1", "Expect ')' after arguments:")]
    [InlineData("1 +")]
    [InlineData("1 + 2 *")]

    public void ExpectExpressionAtEndThrowsCorrectly(string expression, string unexpected = "Expect expression:")
    {
        var tokens = new Scanner(expression).ScanTokens();
        var parser = new Parser(tokens);

        var ex = Assert.Throws<ExpressionException>(() => parser.Parse());

        Assert.Equal($"{unexpected} at end", ex.Message);
    }
}