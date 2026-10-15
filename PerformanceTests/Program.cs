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
    private ExpressionRunner<Test> _expressionTRunner = null!;

    private readonly Dictionary<string, object?> _variables = new()
    {
        { "a", 10d },
        { "b", 20d },
        { "c", 3.5d },
        { "s", "hello" }
    };

    private readonly Test _test = new(); 

    //[Params("abs(-1)", "10+10", "iif(1>2,upper(\"hello\"),upper(\"world\"))", "a+b+c", "(1.4+3.5)/c", "a&b")]
    //[Params("c*c", "10*10", "10-c", "abs(-b)", "1+2+3+4+5+6+7+8+9+10", "a*b*c", "upper(s)")]
    [Params("abs(((a << 3) + (b * 123.456) / (c % 19 + 1.5) - (a * b) % 7.3) + (((~(a & 0xFF)) | (0x7F << (a % 5))) & 65535)) * ((c / (a + b)) - ((a & 3) << 2)) / (1 + (b % 2.5))")]

    public string Code = null!;

    [GlobalSetup]
    public void Setup()
    {
        _expressionRunner = new ExpressionRunner(Code);
        _expressionTRunner = new ExpressionRunner<Test>(Code);
        _astRunner = new AstRunner(Code);
        _astTRunner = new AstRunner<Test>(Code);
    }

    [Benchmark]
    public object? AstFromScratch() => new AstRunner(Code).Interpret(_variables);

    [Benchmark]
    public object? ExprTFromScratch() => new ExpressionRunner<Test>(Code).Interpret(_test);



    [Benchmark(Baseline=true)]
    public object? AstPreCompiled() => _astRunner.Interpret(_variables);

    //[Benchmark]
    //public object? ExprFromScratch() => Runner.RunExpression(Code, _variables);

    [Benchmark]
    public object? ExprPreCompiled() => _expressionRunner.Interpret(_variables);

    [Benchmark]
    public object? AstTPreCompiled() => _astTRunner.Interpret(_test);

    [Benchmark]
    public object? ExprTCompiled() => _expressionTRunner.Interpret(_test);
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
    public string s = "hello";
}