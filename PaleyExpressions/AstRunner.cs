namespace PaleyExpressions;

public class AstRunner(string source, Type? functions)
{
    private static Interpreter? _interpreter;
    private Expr? _expr;

    public void GetAst()
    {
        _expr = Parse(source, functions);
    }

    public object? Interpret(Dictionary<string, object?>? variables)
    {
        _interpreter ??= new Interpreter();

        return _interpreter.Interpret(_expr, variables);
    }

    private static Expr Parse(string source, Type? functions = null)
    {
        var tokens = new Scanner(source).ScanTokens();
        var parser = new Parser(tokens, functions);

        return parser.Parse();
    }
}