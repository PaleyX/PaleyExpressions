using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PaleyExpressions;

namespace PerformanceTests;

public class PreliminaryTest
{
    private Expr _expression = null!;
    private Delegate _compiled = null!;

    [Params("abs(-1)", "10+10", "iif(1>2,10,20)")]
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
    }

    [Benchmark(Baseline = true)]
    public object? AstFromScratch() => Runner.RunAst(Code);

    [Benchmark]
    public object? AstPreCompiled() => new Interpreter().Interpret(_expression);

    [Benchmark]
    public object? ExprFromScratch() => Runner.RunExpression(Code);

    [Benchmark]
    public object? ExprPreCompiled() => _compiled.DynamicInvoke();
}

internal class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<PreliminaryTest>();
    }
}