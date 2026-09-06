using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PaleyExpressions;
using PaleyExpressions.Runners;

namespace PerformanceTests;

public class PreliminaryTest
{
    private Expr _expression = null!;
    private ExpressionRunner _expressionRunner = null!;
    private AstRunner _astRunner = null!;

    private readonly Dictionary<string, object?> _variables = new()
    {
        { "a", 10d },
        { "b", 20d },
        { "c", 3.5d }
    };

    [Params("abs(-1)", "10+10", "iif(1>2,10,20)", "a+b+c", "(1.4+3.5)/c", "a&b")]
    public string Code = null!;

    [GlobalSetup]
    public void Setup()
    {
        var tokens = new Scanner(Code).ScanTokens();
        var parser = new Parser(tokens);
        _expression = parser.Parse();

        _expressionRunner = new ExpressionRunner(Code);
        _astRunner = new AstRunner(Code);
    }

    //[Benchmark(Baseline = true)]
    //public object? AstFromScratch() => Runner.RunAst(Code, _variables);

    [Benchmark(Baseline=true)]
    public object? AstPreCompiled() => _astRunner.Interpret(_variables);

    //[Benchmark]
    //public object? ExprFromScratch() => Runner.RunExpression(Code, _variables);

    [Benchmark]
    public object? ExprPreCompiled() => _expressionRunner.Interpret(_variables);

}

internal class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<PreliminaryTest>();
    }
}