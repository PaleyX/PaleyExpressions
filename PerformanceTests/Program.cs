using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PaleyExpressions;

namespace PerformanceTests
{
    public class PreliminaryTest
    {
        private Expr _expression = null!;

        [Params("abs(-1)", "10+10")]
        public string Code = null!;

        [GlobalSetup]
        public void Setup()
        {
            var tokens = new Scanner(Code).ScanTokens();
            var parser = new Parser(tokens);
            _expression = parser.Parse();
        }

        [Benchmark(Baseline = true)]
        public object? FromScratch() => Runner.Run(Code);

        [Benchmark]
        public object? PreCompiled() => new Interpreter().Interpret(_expression);
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PreliminaryTest>();
        }
    }
}
