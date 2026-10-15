using System.Linq.Expressions;

namespace PaleyExpressions.Visitors.Exp;

internal class ExpressionBuilder : ExpressionBuilderBase
{
    private readonly Dictionary<string, ParameterExpression> _parameters = new();

    public override Expression VisitVariableExpr(Expr.Variable expr)
    {
        if (_parameters.TryGetValue(expr.Name.Lexeme, out var p))
        {
            return p;
        }

        var parameter = Expression.Parameter(typeof(object), expr.Name.Lexeme);

        _parameters.Add(expr.Name.Lexeme, parameter);

        return parameter;
    }

    public List<ParameterExpression> GetParameters() => [.. _parameters.Values];

}