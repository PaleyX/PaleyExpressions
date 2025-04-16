//#define USEPROOF

using System.Linq.Expressions;
using System.Reflection;
using static PaleyExpressions.TokenType;

namespace PaleyExpressions;

internal class ExpressionBuilder : Expr.IVisitor<Expression>
{
    private readonly Dictionary<string, ParameterExpression> _parameters = new();

    public Expression Build(Expr expression)
    {
        return expression.Accept(this);
    }

    public Expression VisitBinaryExpr(Expr.Binary expr)
    {
        var lhs = Build(expr.Left);
        var rhs = Build(expr.Right);

        switch (expr.Operator.TokenType)
        {
            case GREATER:
            {
                var (lhc, rhc) = Conversions<double>(lhs, rhs);
                return Expression.GreaterThan(lhc, rhc);
            }
            case GREATER_EQUAL:
            {
                var (lhc, rhc) = Conversions<double>(lhs, rhs);
                return Expression.GreaterThanOrEqual(lhc, rhc);
            }
            case LESS:
            {
                var (lhc, rhc) = Conversions<double>(lhs, rhs);
                return Expression.LessThan(lhc, rhc);
            }
            case LESS_EQUAL:
            {
                var (lhc, rhc) = Conversions<double>(lhs, rhs);
                return Expression.LessThanOrEqual(lhc, rhc);
            }
            case MINUS:
            {
                var (lhc, rhc) = Conversions<double>(lhs, rhs);
                return Expression.Subtract(lhc, rhc);
            }
            case BANG_EQUAL:
            {
                var equals = typeof(object).GetMethod("Equals", [typeof(object), typeof(object)]);
                var (lhc, rhc) = Conversions<object>(lhs, rhs);
                return Expression.Not(Expression.Call(equals!, [lhc, rhc]));
            }
            case EQUAL_EQUAL:
            {
                var equals = typeof(object).GetMethod("Equals", [typeof(object), typeof(object)]);
                var (lhc, rhc) = Conversions<object>(lhs, rhs);
                return Expression.Call(equals!, [lhc, rhc]);
            }
            case PLUS:
            {
                var plus = typeof(Helpers).GetMethod("Plus", BindingFlags.NonPublic | BindingFlags.Static);
                var (lhc, rhc) = Conversions<object>(lhs, rhs);
                return Expression.Call(plus!, [lhc, rhc]);
            }
            case SLASH:
            {
                var (lhc, rhc) = Conversions<double>(lhs, rhs);
                return Expression.Divide(lhc, rhc);
            }
            case STAR:
            {
                var (lhc, rhc) = Conversions<double>(lhs, rhs);
                return Expression.Multiply(lhc, rhc);
            }
            case MOD:
            {
                var (lhc, rhc) = Conversions<double>(lhs, rhs);
                return Expression.Modulo(lhc, rhc);
            }
        }

        throw new ScannerException("Shouldn't be able to get here");
    }

    public Expression VisitCallExpr(Expr.Call expr)
    {
        var args = new List<Expression>();
        var parameters = expr.Function.GetParameters();

        foreach (var item in expr.Arguments.Select((value, index) => (value, index)))
        {
            var built = Build(item.value);

            var parameterType = parameters[item.index].ParameterType;

            if (parameterType == typeof(Func<object?>))
            {
#if USEPROOF
                var writeLineMethod = typeof(Console).GetMethod("WriteLine", [typeof(string)]);

                // Create a parameter expression for the argument
                var argument = Expression.Constant("Func: " + item.index);

                var converted = Expression.Block
                (
                    Expression.Call(writeLineMethod!, argument),
                    Expression.Convert(built, typeof(object))
                );
#else
                var converted = Expression.Convert(built, typeof(object));
#endif
                var lambda = Expression.Lambda<Func<object>>(converted);
                args.Add(lambda);
                continue;
            }

            var x = new[] { typeof(double), typeof(string), typeof(bool) };

            if (x.Contains(parameterType))
            {
                var cast = Expression.Convert(built, parameterType);
                args.Add(cast);
                continue;
            }

            //else if (parameters[item.index].ParameterType == typeof(double))
            //{
            //    var cast = Expression.Convert(built, typeof(double));
            //    args.Add(cast);
            //}
            //else if (parameters[item.index].ParameterType == typeof(string))
            //{
            //    var cast = Expression.Convert(built, typeof(string));
            //    args.Add(cast);
            //}
            //else if (parameters[item.index].ParameterType == typeof(bool))
            //{
            //    var cast = Expression.Convert(built, typeof(bool));
            //    args.Add(cast);
            //}
            //else
            //{
                args.Add(built);
            //}
        }

        return Expression.Call(expr.Function, [.. args]);
    }

    public Expression VisitGroupingExpr(Expr.Grouping expr) => Build(expr.Expression);

    public Expression VisitLiteralExpr(Expr.Literal expr) => Expression.Constant(expr.Value);

    public Expression VisitLogicalExpr(Expr.Logical expr)
    {
        var lhs = Build(expr.Left);
        var rhs = Build(expr.Right);

        return Expression.Or(lhs, rhs);
    }

    public Expression VisitUnaryExpr(Expr.Unary expr)
    {
        var rhs = Build(expr.Right);

        return expr.Operator.TokenType switch
        {
            BANG => Expression.Not(Expression.Convert(rhs, typeof(double))),
            MINUS => Expression.Negate(Expression.Convert(rhs, typeof(double))),
            _ => throw new ScannerException("Shouldn't be able to get here")
        };
    }

    public Expression VisitVariableExpr(Expr.Variable expr)
    {
        if (_parameters.TryGetValue(expr.Name.Lexeme, out var p))
        {
            return p;
        }

        var parameter = Expression.Parameter(typeof(object), expr.Name.Lexeme);

        _parameters.Add(expr.Name.Lexeme, parameter);

        return parameter;
    }

    public List<ParameterExpression> GetParameters() => _parameters.Values.ToList();

    private static (Expression lhc, Expression rhc) Conversions<T>(Expression lhs, Expression rhs)
    {
        return (Expression.Convert(lhs, typeof(T)), 
                Expression.Convert(rhs, typeof(T)));
    }
}

internal static class Helpers
{
    internal static object Plus(object lhs, object rhs)
    {
        return lhs switch
        {
            double d1 when rhs is double d2 => d1 + d2,
            string s1 when rhs is string s2 => s1 + s2,
            _ => throw new ScannerException("Operands must be two numbers or two strings")
        };
    }
}