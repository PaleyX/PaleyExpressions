using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PaleyExpressions;

namespace PerformanceTests;

public class PreliminaryTest
{
    private Expr _expression = null!;
    private Delegate _compiled = null!;
    private List<object?> _args = null!;

    private readonly Dictionary<string, object?> _variables = new()
    {
        { "a", 10d },
        { "b", 20d },
        { "c", 3.5d }
    };

    [Params("abs(-1)", "10+10", "iif(1>2,10,20)", "a+b+c")]
    public string Code = null!;

    [GlobalSetup]
    public void Setup()
    {
        var tokens = new Scanner(Code).ScanTokens();
        var parser = new Parser(tokens);
        _expression = parser.Parse();

        var builder = new ExpressionBuilder();
        var built = builder.Build(_expression);
        _compiled = Expression.Lambda(built, builder.GetParameters()).Compile();

        _args = Runner.GetExpressionArgs(builder.GetParameters().Select(static p => p.Name), _variables);
    }

    [Benchmark(Baseline = true)]
    public object? AstFromScratch() => Runner.RunAst(Code, _variables);

    [Benchmark]
    public object? AstPreCompiled() => new Interpreter(_variables).Interpret(_expression);

    [Benchmark]
    public object? ExprFromScratch() => Runner.RunExpression(Code, _variables);

    [Benchmark]
    public object? ExprPreCompiled() => _compiled.DynamicInvoke([.. _args]);
}

internal class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<PreliminaryTest>();
    }
}