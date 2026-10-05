using PaleyExpressions;
using System.Diagnostics;
using System.Globalization;

using Runner = PaleyExpressions.Runners.ExpressionRunner;
//using Runner = PaleyExpressions.Runners.AstRunner;

// use this for running a profiler

var stopWatch = new Stopwatch();

stopWatch.Start();
var expr1 = new Runner("x+1");
var expr2 = new Runner("upper(\"Hello \") + tostr(x)", typeof(Functions));
stopWatch.Stop();

Console.WriteLine($"Compile Time: {stopWatch.Elapsed.TotalMilliseconds} ms");

var vars = new Dictionary<string, object?> { ["x"] = 0d };

stopWatch.Start();
for (var i = 0; i < 1000_000; i++)
{
    vars["x"] = expr1.Interpret(vars);
    vars["s"] = expr2.Interpret(vars);
}
stopWatch.Stop();

Console.WriteLine(vars["x"]);
Console.WriteLine(vars["s"]);

Console.WriteLine($"Run Time: {stopWatch.Elapsed.TotalMilliseconds} ms");

public static class Functions
{
    [Function("tostr")]
    public static string ToStr(double d) => Convert.ToString(d, CultureInfo.CurrentCulture);
}