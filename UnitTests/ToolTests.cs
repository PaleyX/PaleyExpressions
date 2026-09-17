using FluentAssertions;
using PaleyExpressions;
using PaleyExpressions.Visitors;
using System.Linq.Expressions;

namespace UnitTests;

public class ToolTests
{
    private readonly List<Type> _functions = [typeof(ToolsTestsFunctions)];

    [Fact]
    public void TooFewArgumentsThrowsCorrectly()
    {
        var ex = Assert.Throws<ExpressionException>(() => Tools.GetFunction(_functions, "f1", []));

        Assert.Equal("Function 'f1': argument count mismatch", ex.Message);
    }

    [Fact]
    public void OneArgWithNoParamsProvidedIsOk()
    {
        var args = new List<Expr>
        {
            new Expr.Literal("First")
        };

        var func = Tools.GetFunction(_functions, "f1", args);

        // Really, the test is that an exception is not thrown
        func.Name.Should().Be(nameof(ToolsTestsFunctions.F1));

        var expr = new Expr.Call(null!, null!, args, func);

        // interpreter
        var interpreter = new AstInterpreter();
        var result = interpreter.VisitCallExpr(expr);
        result.Should().Be("First");

        // Expressions tree
        var builder = new ExpressionBuilder();
        var built = builder.VisitCallExpr(expr);
        var compiled = Expression.Lambda(built, builder.GetParameters()).Compile();
        result = compiled.DynamicInvoke();

        result.Should().Be("First");
    }

    [Fact]
    public void NoArgumentsPassedToPurelyParamsIsOk()
    {
        var func = Tools.GetFunction(_functions, "f2", []);

        // Really, the test is that an exception is not thrown
        func.Name.Should().Be(nameof(ToolsTestsFunctions.F2));

        var expr = new Expr.Call(null!, null!, [], func);

        // Interpreter
        var interpreter = new AstInterpreter();
        var result = interpreter.VisitCallExpr(expr);
        result.Should().Be("");

        // Expressions tree
        var builder = new ExpressionBuilder();
        var built = builder.VisitCallExpr(expr);
        var compiled = Expression.Lambda(built, builder.GetParameters()).Compile();
        result = compiled.DynamicInvoke(null);
        result.Should().Be("");
    }
}

public static class ToolsTestsFunctions
{
    [Function("f1")]
    public static string F1(string text, params Func<object>[] args)
    {
        text += " " + string.Join(" ", args.Select(a => a()));

        return text.Trim();
    }

    [Function("f2")]
    public static string F2(params Func<object>[] args)
    {
        return string.Join(" ", args.Select(a => a()));
    }
}