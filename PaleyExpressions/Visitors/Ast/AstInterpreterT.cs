using System.Reflection;

namespace PaleyExpressions.Visitors.Ast;

internal class AstInterpreter<T> : AstInterpreterBase where T : class
{
    private T? _context;
    private readonly Dictionary<string, FieldInfo?> _fields = new();

    public object? Interpret(Expr expression, T context)
    {
        _context = context;

        return Evaluate(expression);
    }

    public override object? VisitVariableExpr(Expr.Variable expr)
    {
        if (_context is null)
        {
            throw new ExpressionException($"Unknown variable '{expr.Name.Lexeme}'");
        }

        if (!_fields.TryGetValue(expr.Name.Lexeme, out var field))
        {
            // Try to get a field
            field = typeof(T).GetField(expr.Name.Lexeme,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            _fields.Add(expr.Name.Lexeme, field);
        }

        if (field != null)
        {
            return field.GetValue(_context);
        }

        throw new ExpressionException($"Unknown variable '{expr.Name.Lexeme}'");
    }
}

