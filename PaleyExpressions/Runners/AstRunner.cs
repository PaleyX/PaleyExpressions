using PaleyExpressions.Visitors;

namespace PaleyExpressions.Runners;

public class AstRunner(string source, Type? functions = null) : IRunner
{
    private Expr? _expr;
    private AstInterpreter? _interpreter;

    public object? Interpret(Dictionary<string, object?>? variables = null)
    {
        _expr ??= Helpers.Parse(source, functions);
        _interpreter ??= new AstInterpreter();

        return _interpreter.Interpret(_expr, variables);
    }
}