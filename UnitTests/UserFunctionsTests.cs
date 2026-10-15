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

    [Theory]
    [InlineData(nameof(UserFunctions.ReturnsVoid))]
    [InlineData(nameof(UserFunctions.ReturnsInt))]
    [InlineData(nameof(UserFunctions.ReturnsFloat))]
    public void UnallowedAsReturnTypesThrowCorrectly(string name)
    {
        var expression = $"{name}()";

        var ex1 = Assert.Throws<ExpressionException>(() => new AstRunner(expression, typeof(UserFunctions)).Interpret());
        var ex2 = Assert.Throws<ExpressionException>(() => new ExpressionRunner(expression, typeof(UserFunctions)).Interpret());

        Assert.Equal(ex1.Message, ex2.Message);
        Assert.Equal($"Callable function '{name}' has unallowed return type", ex1.Message);
    }

    [Theory]
    [InlineData(nameof(UserFunctions.TakesInt), typeof(int))]
    [InlineData(nameof(UserFunctions.TakesFloat), typeof(float))]
    public void UnallowedParameterTypesThrowCorrectly(string name, Type type)
    {
        var expression = $"{name}(10)";

        var ex1 = Assert.Throws<ExpressionException>(() => new AstRunner(expression, typeof(UserFunctions)).Interpret());
        var ex2 = Assert.Throws<ExpressionException>(() => new ExpressionRunner(expression, typeof(UserFunctions)).Interpret());

        Assert.Equal(ex1.Message, ex2.Message);
        Assert.Equal($"Callable function '{name}' has unallowed parameter of type {type}", ex1.Message);
    }
}

public static class UserFunctions
{
    [Function("abs")]
    public static double Abs(double d) => 1_000_000;

    [Function(nameof(ReturnsVoid))]
    public static void ReturnsVoid() { }

    [Function(nameof(ReturnsInt))]
    public static int ReturnsInt() => 1;

    [Function(nameof(ReturnsFloat))]
    public static float ReturnsFloat() => 1;

    [Function(nameof(TakesInt))]
    public static double TakesInt(int i) => i;

    [Function(nameof(TakesFloat))]
    public static double TakesFloat(float i) => i;
}

