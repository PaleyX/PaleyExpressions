namespace PaleyExpressions.Visitors.Ast;

internal class AstInterpreter : AstInterpreterBase
{
    private Dictionary<string, object?>? _variables;

    public object? Interpret(Expr expression, Dictionary<string, object?>? variables)
    {
        _variables = variables;

        return Evaluate(expression);
    }

    public override object? VisitVariableExpr(Expr.Variable expr)
    {
        if (_variables != null && _variables.TryGetValue(expr.Name.Lexeme, out var value))
        {
            return value;
        }

        throw new ExpressionException($"Unknown variable '{expr.Name.Lexeme}'");
    }
}