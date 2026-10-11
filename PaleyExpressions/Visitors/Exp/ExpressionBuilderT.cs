using System.Linq.Expressions;
using System.Reflection;

namespace PaleyExpressions.Visitors.Exp;

internal class ExpressionBuilder<T> : ExpressionBuilderBase where T : class
{
    internal ParameterExpression? Context
    {
        get;
    } = Expression.Parameter(typeof(T), "context");

    public override Expression VisitVariableExpr(Expr.Variable expr)
    {
        var mi = typeof(T).GetMember(expr.Name.Lexeme,
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance).FirstOrDefault();

        if (mi == null)
        {
            throw new ExpressionException($"Field or property '{expr.Name.Lexeme}' not found.");
        }

        return mi.MemberType switch
        {
            MemberTypes.Field => Expression.Field(Context, (mi as FieldInfo)!),
            MemberTypes.Property => Expression.Property(Context, (mi as PropertyInfo)!),
            _ => throw new ExpressionException($"Unsupported member type '{mi.MemberType}' for '{expr.Name.Lexeme}'")
        };
   }
}