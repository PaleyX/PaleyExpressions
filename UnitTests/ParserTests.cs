using System.Reflection;
using PaleyExpressions;

namespace UnitTests;

public class ParserTests
{
    [Fact]
    public void AstIsCreatedCorrectlyForGrouping()
    {
        var tokens = new Scanner("10*(6+5)").ScanTokens();
        var parser = new Parser(tokens);

        var expression = parser.Parse();

        // Top level
        var binary = Assert.IsType<Expr.Binary>(expression);
        Assert.Equal(TokenType.STAR, binary.Operator.TokenType);

        // LHS
        var literal = Assert.IsType<Expr.Literal>(binary.Left);
        var value = Assert.IsType<double>(literal.Value);
        Assert.Equal(10d, value);

        // RHS
        var grouping = Assert.IsType<Expr.Grouping>(binary.Right);
        binary = Assert.IsType<Expr.Binary>(grouping.Expression);

        // Grouping top level
        Assert.Equal(TokenType.PLUS, binary.Operator.TokenType);

        // Grouping LHS
        literal = Assert.IsType<Expr.Literal>(binary.Left);
        value = Assert.IsType<double>(literal.Value);
        Assert.Equal(6d, value);

        // Grouping RHS
        literal = Assert.IsType<Expr.Literal>(binary.Right);
        value = Assert.IsType<double>(literal.Value);
        Assert.Equal(5d, value);
    }

    [Fact]
    public void AstIsCreatedCorrectlyForBuiltinFunctionCall()
    {
        var tokens = new Scanner("10.76*abs(2-a)").ScanTokens();
        var parser = new Parser(tokens);

        var expression = parser.Parse();

        // Top level
        var binary = Assert.IsType<Expr.Binary>(expression);
        Assert.Equal(TokenType.STAR, binary.Operator.TokenType);

        // LHS
        var literal = Assert.IsType<Expr.Literal>(binary.Left);
        var value = Assert.IsType<double>(literal.Value);
        Assert.Equal(10.76d, value);

        // RHS
        var call = Assert.IsType<Expr.Call>(binary.Right);
        // Function name is a variable (an identifier)
        var variable = Assert.IsType<Expr.Variable>(call.Callee);
        Assert.Equal(TokenType.IDENTIFIER, variable.Name.TokenType);
        Assert.Equal("abs", variable.Name.Lexeme);
        Assert.Null(variable.Name.Literal);

        // RHS - call arguments - only 1
        Assert.Single(call.Arguments);

        // RHS - call method
        // the actual type is RuntimeMethodInfo
        var method = Assert.IsAssignableFrom<MethodInfo>(call.Function);
        Assert.Equal("Abs", method.Name);
        Assert.Equal("Builtins", method.DeclaringType?.Name);

        Assert.Equal(method.GetParameters().Length, call.Arguments.Count);

        // RHS - argument
        var argument = call.Arguments.First();
        binary = Assert.IsType<Expr.Binary>(argument);

        Assert.Equal(TokenType.MINUS, binary.Operator.TokenType);

        // argument LHS
        literal = Assert.IsType<Expr.Literal>(binary.Left);
        value = Assert.IsType<double>(literal.Value);
        Assert.Equal(2d, value);

        // argument RHS
        variable = Assert.IsType<Expr.Variable>(binary.Right);
        Assert.Equal(TokenType.IDENTIFIER, variable.Name.TokenType);
        Assert.Equal("a", variable.Name.Lexeme);
        Assert.Null(variable.Name.Literal);
    }

    [Theory]
    [InlineData("1!", "!")]
    public void UnexpectedTokenThrowsCorrectly(string expression, string unexpected)
    {
        var tokens = new Scanner(expression).ScanTokens();
        var parser = new Parser(tokens);

        var ex = Assert.Throws<ExpressionException>(parser.Parse);

        Assert.Equal($"Unexpected token: at '{unexpected}'", ex.Message);
    }

    [Theory]
    [InlineData("(,)", ",")]
    public void ExpectExpressionThrowsCorrectly(string expression, string unexpected)
    {
        var tokens = new Scanner(expression).ScanTokens();
        var parser = new Parser(tokens);

        var ex = Assert.Throws<ExpressionException>(parser.Parse);

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

        var ex = Assert.Throws<ExpressionException>(parser.Parse);

        Assert.Equal($"{unexpected} at end", ex.Message);
    }
}