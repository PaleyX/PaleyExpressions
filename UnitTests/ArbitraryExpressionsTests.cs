using PaleyExpressions;

namespace UnitTests;

public class ArbitraryExpressionsTests
{
    private readonly Dictionary<string, object?> _variables = new()
    {
        { "a", 10d },
        { "b", 20d },
        { "c", 3.5d },
        { "d", -12.894d },
        { "pi", Math.PI },
        { "s", "Hello" },
        { "bt", true },
        { "bf", false }
    };

    [Theory]
    [InlineData("10", 10d)]
    [InlineData("10+10", 20d)]
    [InlineData("-19", -19d)]
    [InlineData("--23", 23d)]
    [InlineData("\"Hello\"", "Hello")]
    [InlineData("\"Hello \"+\"World\"", "Hello World")]
    [InlineData("10/(9-1)", 1.25d)]
    [InlineData("a+b+c", 33.5d)]
    [InlineData("(b/a)+c", 5.5d)]
    [InlineData("pi>3", true)]
    [InlineData("nil", null)]
    public void ArbitraryExpressions(string expression, object? expected)
    {
        var expressionsResult = Runner.RunExpression(expression, _variables);
        var astResult = Runner.RunAst(expression, _variables);

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
    }
        
    [Theory]
    [InlineData("upper(s)", "HELLO")]
    [InlineData("lower(s)", "hello")]
    [InlineData("abs(-19)", 19d)]
    [InlineData("abs(-23.897)", 23.897d)]
    [InlineData("abs(d)", 12.894d)]
    [InlineData("-abs(-a)", -10d)]
    [InlineData("iif(1>2,10,20)", 20d)]
    [InlineData("iif(1<2,10,20)", 10d)]
    [InlineData("iif(bt,10,20)", 10d)]
    [InlineData("iif(bf,10,20)", 20d)]
    [InlineData("iif(upper(s)==\"HELLO\", abs(-10), abs(-20))", 10d)]
    public void BuiltinsExpressions(string expression, object? expected)
    {
        var expressionsResult = Runner.RunExpression(expression, _variables);
        var astResult = Runner.RunAst(expression, _variables);

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
    }
}