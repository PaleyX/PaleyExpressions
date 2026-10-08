using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PaleyExpressions.Runners;

namespace PerformanceTests;

[MemoryDiagnoser]
public class PreliminaryTest
{
    private ExpressionRunner _expressionRunner = null!;
    private AstRunner _astRunner = null!;
    private AstRunner<Test> _astTRunner = null!;

    private readonly Dictionary<string, object?> _variables = new()
    {
        { "a", 10d },
        { "b", 20d },
        { "c", 3.5d }
    };

    private readonly Test _test = new(); 

    //[Params("abs(-1)", "10+10", "iif(1>2,upper(\"hello\"),upper(\"world\"))", "a+b+c", "(1.4+3.5)/c", "a&b")]
    [Params("c*c", "10*10", "10-c", "10+10", "1+2+3+4+5+6+7+8+9+10", "a*b*c")]

    public string Code = null!;

    [GlobalSetup]
    public void Setup()
    {
        _expressionRunner = new ExpressionRunner(Code);
        _astRunner = new AstRunner(Code);
        _astTRunner = new AstRunner<Test>(Code);
    }

    //[Benchmark(Baseline = true)]
    //public object? AstFromScratch() => Runner.RunAst(Code, _variables);

    [Benchmark(Baseline=true)]
    public object? AstPreCompiled() => _astRunner.Interpret(_variables);

    //[Benchmark]
    //public object? ExprFromScratch() => Runner.RunExpression(Code, _variables);

    [Benchmark]
    public object? ExprPreCompiled() => _expressionRunner.Interpret(_variables);

    [Benchmark]
    public object? AstTPreCompiled() => _astTRunner.Interpret(_test);
}

internal class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<PreliminaryTest>();
    }
}

internal class Test
{
    public double a = 10;
    public double b = 20;
    public double c = 3.5;
}