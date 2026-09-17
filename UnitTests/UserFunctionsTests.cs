using PaleyExpressions;
using PaleyExpressions.Runners;

namespace UnitTests;

public class UserFunctionsTests
{
    [Fact]
    public void UserFunctionReplacesBuiltin()
    {
        const string expression = "abs(-1)";

        // Builtin works
        var astResult = new AstRunner(expression).Interpret();
        var expResult = new ExpressionRunner(expression).Interpret();

        Assert.Equal(1d, astResult);
        Assert.Equal(1d, expResult);

        // Use user defined version of abs
        astResult = new AstRunner(expression, typeof(UserFunctions)).Interpret();
        expResult = new ExpressionRunner(expression, typeof(UserFunctions)).Interpret();

        Assert.Equal(1_000_000d, astResult);
        Assert.Equal(1_000_000d, expResult);
    }
}

public static class UserFunctions
{
    [Function("abs")]
    public static double Abs(double d) => 1_000_000;
}

