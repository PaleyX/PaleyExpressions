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
    [InlineData("true and false", false)]
    [InlineData("true or false", true)]
    [InlineData("true and (2 == 2)", true)]
    [InlineData("1&1", 1d)]
    [InlineData("1|1", 1d)]
    [InlineData("1&2", 0d)]
    [InlineData("1234&4321", (double)(1234 & 4321))]
    [InlineData("1234|4321", (double)(1234 | 4321))]
    public void ArbitraryExpressions(string expression, object? expected)
    {
        var expressionsResult = Runner.RunExpression(expression, _variables);
        var astResult = Runner.RunAst(expression, _variables);

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
    }

    [Theory]
    [InlineData("1 << 2", 4d)]
    [InlineData("1 << (1 << 2)", 16d)]
    [InlineData("\"hello\" << 1", "ello")]
    [InlineData("\"hello\" << 3", "lo")]
    [InlineData("s << 1", "ello")]
    [InlineData("s << 3", "lo")]
    public void LeftShiftTests(string expression, object? expected)
    {
        var expressionsResult = Runner.RunExpression(expression, _variables);
        var astResult = Runner.RunAst(expression, _variables);

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
    }

    [Theory]
    [InlineData("1 >> 2", 0d)]
    [InlineData("16 >> 2", 4d)]
    [InlineData("\"hello\" >> 1", "hell")]
    [InlineData("\"hello\" >> 3", "he")]
    [InlineData("s >> 1", "Hell")]
    [InlineData("s >> 3", "He")]
    public void RightShiftTests(string expression, object? expected)
    {
        var expressionsResult = Runner.RunExpression(expression, _variables);
        var astResult = Runner.RunAst(expression, _variables);

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
    }

    [Theory]
    // LeftShift
    [InlineData("true << false")]
    [InlineData("2 << \"hello\"")]
    [InlineData("false << 2")]
    [InlineData("nil<<2")]
    [InlineData("2<<nil")]
    // RightShift
    [InlineData("true >> false")]
    [InlineData("2 >> \"hello\"")]
    [InlineData("false >> 2")]
    [InlineData("nil>>2")]
    [InlineData("2>>nil")]
    public void ShiftThrowsCorrectly_BadTypes(string expression)
    {
        var ex = Assert.Throws<ScannerException>(() => Runner.RunExpression(expression, _variables));
        var ast = Assert.Throws<ScannerException>(() => Runner.RunAst(expression, _variables));

        Assert.Equal(ex.Message, ast.Message);
        Assert.Equal("Operands must be 2 numbers or a string and a number", ex.Message);
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