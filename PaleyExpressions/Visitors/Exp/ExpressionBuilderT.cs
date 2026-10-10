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
        var fieldInfo = typeof(T).GetField(expr.Name.Lexeme,
            BindingFlags.Public | 
            BindingFlags.NonPublic |
            BindingFlags.Instance);

        if (fieldInfo == null)
        {
            throw new ExpressionException($"Field '{expr.Name.Lexeme}' not found.");
        }

        return Expression.Field(Context, fieldInfo);
   }
}