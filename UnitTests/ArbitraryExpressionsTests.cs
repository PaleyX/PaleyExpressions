using PaleyExpressions;
using PaleyExpressions.Runners;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions.Specialized;
using Microsoft.Testing.Platform.Extensions;

namespace UnitTests;

// Suppress CSharpSquid S4144: "Methods should not have
// identical implementations" because the methods are
// testing different things, but the implementation is
// the same.
[SuppressMessage("Maintainability", "S4144")]
public class ArbitraryExpressionsTests
{
    private readonly Dictionary<string, object?> _variables = new()
    {
        { "a", 10d },
        { "b", 20d },
        { "c", 3.5d },
        { "d", -12.894d },
        { "n1", 1d },
        { "n2", 2d },
        { "n3", 3d },
        { "n255", 255d },
        { "pi", Math.PI },
        { "s", "Hello" },
        { "bt", true },
        { "bf", false },
        { "s1", "A" },
        { "s2", "B" },
        { "s3", "C" }
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
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("!(1>2)", true)]
    [InlineData("1&1", 1d)]
    [InlineData("1|1", 1d)]
    [InlineData("1&2", 0d)]
    [InlineData("n1|n2", 3d)]
    [InlineData("n1&n2", 0d)]
    [InlineData("n2&n3", 2d)]
    [InlineData("1234&4321", (double)(1234 & 4321))]
    [InlineData("1234|4321", (double)(1234 | 4321))]
    [InlineData("~255", (double)~(uint)255d)]
    [InlineData("n2&~n255", 0d)]
    [InlineData("~10|255", (double)(~(uint)10d | (uint)255d))]
    [InlineData("10+~1000", (double)((uint)10d + ~(uint)1000d))]
    [InlineData("bt and bf", false)]
    [InlineData("bf and bt", false)]
    [InlineData("bf or bt", true)]
    [InlineData("bt or bf", true)]
    [InlineData("bt", true)]
    [InlineData("bf", false)]
    [InlineData("\"Hello\" + \"World\"", "HelloWorld")]
    [InlineData("s1 + s2", "AB")]
    [InlineData("s1 + s2 + s3", "ABC")]
    public void ArbitraryExpressions(string expression, object? expected)
    {
        var test = new Test();

        var astResult = new AstRunner(expression).Interpret(_variables);
        var astTResult = new AstRunner<Test>(expression).Interpret(test);

        var expressionsResult = new ExpressionRunner(expression).Interpret(_variables);
        var expressionTResult = new ExpressionRunner<Test>(expression).Interpret(test);

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(astResult, astTResult);
        Assert.Equal(astResult, expressionTResult);

        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
        Assert.Equal(expected, astTResult);
        Assert.Equal(expected, expressionTResult);
    }

    [Theory]
    [InlineData("1 << 2", 4d)]
    [InlineData("1 << (1 << 2)", 16d)]
    [InlineData("\"hello\" << 1", "ello")]
    [InlineData("\"hello\" << 3", "lo")]
    [InlineData("s << 1", "ello")]
    [InlineData("s << 3", "lo")]
    [InlineData("n1 << 1", 2d)]
    [InlineData("n2 << 2", 8d)]
    public void LeftShiftTests(string expression, object? expected)
    {
        var test = new Test();

        var astResult = new AstRunner(expression).Interpret(_variables);
        var astTResult = new AstRunner<Test>(expression).Interpret(test);

        var expressionsResult = new ExpressionRunner(expression).Interpret(_variables);
        var expressionsTResult = new ExpressionRunner<Test>(expression).Interpret(test);

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(astResult, astTResult);
        Assert.Equal(astResult, expressionsTResult);

        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
        Assert.Equal(expected, astTResult);
        Assert.Equal(expected, expressionsTResult);
    }

    [Theory]
    [InlineData("1 >> 2", 0d)]
    [InlineData("16 >> 2", 4d)]
    [InlineData("\"hello\" >> 1", "hell")]
    [InlineData("\"hello\" >> 3", "he")]
    [InlineData("s >> 1", "Hell")]
    [InlineData("s >> 3", "He")]
    [InlineData("n1 >> 1", 0d)]
    [InlineData("n2 >> 1", 1d)]

    public void RightShiftTests(string expression, object? expected)
    {
        var test = new Test();

        var astResult = new AstRunner(expression).Interpret(_variables);
        var astTResult = new AstRunner<Test>(expression).Interpret(test);

        var expressionsResult = new ExpressionRunner(expression).Interpret(_variables);
        var expressionsTResult = new ExpressionRunner<Test>(expression).Interpret(test);

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(astResult, astTResult);
        Assert.Equal(astResult, expressionsTResult);

        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
        Assert.Equal(expected, astTResult);
        Assert.Equal(expected, expressionsTResult);
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
        var ex = Assert.Throws<ExpressionException>(() => new ExpressionRunner(expression).Interpret(_variables));
        var expT = Assert.Throws<ExpressionException>(() => new ExpressionRunner<Test>(expression).Interpret(new Test()));
        var ast = Assert.Throws<ExpressionException>(() => new AstRunner(expression).Interpret(_variables));
        var astT = Assert.Throws<ExpressionException>(() => new AstRunner<Test>(expression).Interpret(new Test()));

        Assert.Equal(ex.Message, ast.Message);
        Assert.Equal(ex.Message, astT.Message);
        Assert.Equal(ex.Message, expT.Message);
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
    [InlineData("""iif(upper(s)=="HELLO", abs(-10), abs(-20))""", 10d)]
    [InlineData("""cond(1>2,"A",1<2,"B")""", "B")]
    [InlineData("""cond(1>2,"A")""", null)]
    [InlineData("""cond(1>2,"A",1==2,"B", d<15,upper(s))""", "HELLO")]
    public void BuiltinsExpressions(string expression, object? expected)
    {
        var astResult = new AstRunner(expression).Interpret(_variables);
        var astTResult = new AstRunner<Test>(expression).Interpret(new Test());

        var expressionsResult = new ExpressionRunner(expression).Interpret(_variables);
        var expressionsTResult = new ExpressionRunner<Test>(expression).Interpret(new Test());

        Assert.Equal(astResult, expressionsResult);
        Assert.Equal(astResult, astTResult);
        Assert.Equal(astResult, expressionsTResult);

        Assert.Equal(expected, astResult);
        Assert.Equal(expected, expressionsResult);
        Assert.Equal(expected, astTResult);
        Assert.Equal(expected, expressionsTResult);
    }

    [Fact]
    public void VariableTypeChangesOnPlusWork()
    {
        Dictionary<string, object?> vars = new()
        {
            { "a", 10d },
            { "b", 20d }
        };

        const string expression = "a+b";

        var astRunner = new AstRunner(expression);
        var astTRunner = new AstRunner<Changes1>(expression);

        var expRunner = new ExpressionRunner(expression);
        var expTRunner = new ExpressionRunner<Changes1>(expression);

        var changes = new Changes1
        {
            a = 10d,
            b = 20d
        };

        // numeric values
        var result1 = astRunner.Interpret(vars);
        var result2 = expRunner.Interpret(vars);
        var result3 = astTRunner.Interpret(changes);
        var result4 = expTRunner.Interpret(changes);
        Assert.Equal(30d, result1);
        Assert.Equal(result1, result2);
        Assert.Equal(result1, result3);
        Assert.Equal(result1, result4);

        // string values
        vars["a"] = "hello";
        vars["b"] = " world";

        changes.a = "hello";
        changes.b = " world";

        result1 = astRunner.Interpret(vars);
        result2 = expRunner.Interpret(vars);
        result3 = astTRunner.Interpret(changes);
        result4 = expTRunner.Interpret(changes);
        Assert.Equal("hello world", result1);
        Assert.Equal(result1, result2);
        Assert.Equal(result1, result3);
        Assert.Equal(result1, result4);
    }

    [Fact]
    public void VariableValuesChangingWorks()
    {
        Dictionary<string, object?> astVars = new()
        {
            { "a", 0d },
            { "b", 100d },
            { "s", "0" }
        };

        var expVars = astVars.ToDictionary(entry => entry.Key, entry => entry.Value);

        var astInc = new AstRunner("a+1");
        var astDec = new AstRunner("b-1");

        var expInc = new ExpressionRunner("a+1");
        var expDec = new ExpressionRunner("b-1");

        var astTInc = new AstRunner<Changes2>("a+1");
        var astTDec = new AstRunner<Changes2>("b-1");

        var expTInc = new ExpressionRunner<Changes2>("a+1");
        var expTDec = new ExpressionRunner<Changes2>("b-1");

        var astToStr = new AstRunner("tostr(a)", typeof(TestFunctions));
        var astTToStr = new AstRunner<Changes2>("tostr(a)", typeof(TestFunctions));
        var expToStr = new ExpressionRunner("tostr(a)", typeof(TestFunctions));
        var expTToStr = new ExpressionRunner<Changes2>("tostr(a)", typeof(TestFunctions));

        var changesAst = new Changes2();
        var changesExp = new Changes2();

        for (var i = 0; i < 100; ++i)
        {
            astVars["a"] = astInc.Interpret(astVars);
            astVars["b"] = astDec.Interpret(astVars);

            expVars["a"] = expInc.Interpret(expVars);
            expVars["b"] = expDec.Interpret(expVars);

            changesAst.a = (double)astTInc.Interpret(changesAst)!;
            changesAst.b = (double)astTDec.Interpret(changesAst)!;

            changesExp.a = (double)expTInc.Interpret(changesExp)!;
            changesExp.b = (double)expTDec.Interpret(changesExp)!;

            Assert.Equal(astVars["a"], expVars["a"]);
            Assert.Equal(astVars["b"], expVars["b"]);
            Assert.Equal(expVars["a"], changesAst.a);
            Assert.Equal(expVars["b"], changesAst.b);
            Assert.Equal(expVars["a"], changesExp.a);
            Assert.Equal(expVars["b"], changesExp.b);

            astVars["s"] = astToStr.Interpret(astVars);
            expVars["s"] = expToStr.Interpret(expVars);
            changesAst.s = (string)astTToStr.Interpret(changesAst)!;
            changesExp.s = (string)expTToStr.Interpret(changesExp)!;

            Assert.Equal(astVars["s"], expVars["s"]);
            Assert.Equal(expVars["s"], changesAst.s);
            Assert.Equal(expVars["s"], changesExp.s);
        }

        Assert.Equal(100d, astVars["a"]);
        Assert.Equal(0d, astVars["b"]);

        Assert.Equal(100d, expVars["a"]);
        Assert.Equal(0d, expVars["b"]);

        Assert.Equal(100d, changesAst.a);
        Assert.Equal(0d, changesAst.b);

        Assert.Equal(100d, changesExp.a);
        Assert.Equal(0d, changesExp.b);

        Assert.Equal("100", astVars["s"]);
        Assert.Equal("100", expVars["s"]);
        Assert.Equal("100", changesAst.s);
        Assert.Equal("100", changesExp.s);
    }
}

public static class TestFunctions
{
    [Function("tostr")]
    public static string ToStr(double value) => value.ToString();
}

public class Test
{
    public double a = 10d;
    public double b = 20d;
    public double c = 3.5d;
    public double d = -12.894d;
    public double n1 = 1d;
    public double n2 = 2d;
    public double n3 = 3d;
    public double n255 = 255d;
    public double pi = Math.PI;
    public string s = "Hello";
    public bool bt = true;
    public bool bf = false;
    public string s1 = "A";
    public string s2 = "B";
    public string s3 = "C";
}

public class Changes1
{
    public object? a;
    public object? b;
}

public class Changes2
{
    public double a;
    public double b = 100;
    public string s = "0";
}
