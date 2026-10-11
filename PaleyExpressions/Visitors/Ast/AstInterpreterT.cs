using System.Diagnostics;
using System.Reflection;

namespace PaleyExpressions.Visitors.Ast;

internal class AstInterpreter<T> : AstInterpreterBase where T : class
{
    private T? _context;
    private readonly Dictionary<string, IGetValue<T>> _getters = new();

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

        if (!_getters.TryGetValue(expr.Name.Lexeme, out var getter))
        {
            var mi = typeof(T).GetMember(expr.Name.Lexeme,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance).FirstOrDefault();

            if (mi == null)
            {
                throw new ExpressionException($"Unknown variable or property '{expr.Name.Lexeme}'");
            }

            getter = mi.MemberType switch
            {
                MemberTypes.Field => new GetFieldValue<T>((mi as FieldInfo)!),
                MemberTypes.Property => new GetPropertyValue<T>((mi as PropertyInfo)!),
                _ => throw new ExpressionException($"Unsupported member type '{mi.MemberType}' for '{expr.Name.Lexeme}'")
            };

            _getters.Add(expr.Name.Lexeme, getter);
        }

        return getter.GetValue(_context);
    }
}

internal interface IGetValue<in T>
{
    object? GetValue(T context);
}

internal class GetFieldValue<T>(FieldInfo fieldInfo) : IGetValue<T>
{
    public object? GetValue(T context)
    {
        return fieldInfo.GetValue(context);
    }
}

internal class GetPropertyValue<T>(PropertyInfo propertyInfo) : IGetValue<T>
{
    public object? GetValue(T context)
    {
        return propertyInfo.GetValue(context);
    }
}

