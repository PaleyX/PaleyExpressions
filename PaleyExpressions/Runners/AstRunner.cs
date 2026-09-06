using PaleyExpressions.Visitors;

namespace PaleyExpressions.Runners;

public class AstRunner(string source, Type? functions = null) : IRunner
{
    private Expr? _expr;

    public object? Interpret(Dictionary<string, object?>? variables = null)
    {
        _expr ??= Helpers.Parse(source, functions);

        var interpreter = new AstInterpreter();

        return interpreter.Interpret(_expr, variables);
    }
}