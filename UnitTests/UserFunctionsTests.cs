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

        // Use user defined version of abs replaces version in Builtins
        astResult = new AstRunner(expression, typeof(UserFunctions)).Interpret();
        expResult = new ExpressionRunner(expression, typeof(UserFunctions)).Interpret();

        Assert.Equal(1_000_000d, astResult);
        Assert.Equal(1_000_000d, expResult);

        // upper function in Builtins is still accessible 
        const string upper = "upper(\"hello\")";

        astResult = new AstRunner(upper, typeof(UserFunctions)).Interpret();
        expResult = new ExpressionRunner(upper, typeof(UserFunctions)).Interpret();

        Assert.Equal("HELLO", astResult);
        Assert.Equal("HELLO", expResult);
    }
}

public static class UserFunctions
{
    [Function("abs")]
    public static double Abs(double d) => 1_000_000;
}

