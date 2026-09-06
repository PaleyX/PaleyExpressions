namespace PaleyExpressions.Runners;

internal static class Helpers
{
    internal static Expr Parse(string source, Type? functions = null)
    {
        var tokens = new Scanner(source).ScanTokens();
        var parser = new Parser(tokens, functions);

        return parser.Parse();
    }
}

