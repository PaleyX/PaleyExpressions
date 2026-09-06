using System.Linq.Expressions;
using System.Reflection;
using static PaleyExpressions.TokenType;

namespace PaleyExpressions.Visitors;

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
                return Expression.Not(Expression.Call(equals!, lhc, rhc));
            }
            case EQUAL_EQUAL:
            {
                var equals = typeof(object).GetMethod("Equals", [typeof(object), typeof(object)]);
                var (lhc, rhc) = Conversions<object>(lhs, rhs);
                return Expression.Call(equals!, lhc, rhc);
            }
            case PLUS:
            {
                var plus = typeof(Helpers).GetMethod("Plus", BindingFlags.NonPublic | BindingFlags.Static);
                var (lhc, rhc) = Conversions<object>(lhs, rhs);
                return Expression.Call(plus!, lhc, rhc);
            }
            case LEFT_SHIFT:
            case RIGHT_SHIFT:
            {
                var proc = expr.Operator.TokenType == LEFT_SHIFT ? "LeftShift" : "RightShift";
                var shift = typeof(Helpers).GetMethod(proc, BindingFlags.NonPublic | BindingFlags.Static);
                var (lhc, rhc) = Conversions<object>(lhs, rhs);
                return Expression.Call(shift!, lhc, rhc);
            }
            case BITWISE_AND:
            {
                var (lhc, rhc) = Conversions<uint>(lhs, rhs);
                return Expression.Convert(Expression.And(lhc, rhc), typeof(double));
            }
            case BITWISE_OR:
            {
                var (lhc, rhc) = Conversions<uint>(lhs, rhs);
                return Expression.Convert(Expression.Or(lhc, rhc), typeof(double));
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

        throw new ExpressionException("Shouldn't be able to get here");
    }

    public Expression VisitCallExpr(Expr.Call expr)
    {
        var args = new List<Expression>();
        var parameters = expr.Function.GetParameters();

        var last = parameters.LastOrDefault();
        var paramsAdded = false;

        foreach (var item in expr.Arguments.Select((value, index) => (value, index)))
        {
            var parameter = parameters[item.index];

            var isParams = parameter.IsDefined(typeof(ParamArrayAttribute), false);

            if (isParams)
            {
                paramsAdded = true;

                var paramType = parameter.ParameterType.GetElementType();

                var array = Expression.NewArrayInit(paramType!, [.. expr.Arguments.Skip(item.index).Select(a => GetParameter(paramType, Build(a)))]);
                args.Add(array);
                break;
            }

            args.Add(GetParameter(parameter.ParameterType, Build(item.value)));
        }

        // if function has a params but the call doesnt have any parameters,
        // add an empty array
        if (last != null && last.IsDefined(typeof(ParamArrayAttribute), false))
        {
            if (!paramsAdded)
            {
                var paramType = last.ParameterType.GetElementType();
                var array = Expression.NewArrayInit(paramType!);
                args.Add(array);
            }
        }

        return Expression.Call(expr.Function, args);

        static Expression GetParameter(Type parameterType, Expression expression)
        {
            var converted = Expression.Convert(expression, typeof(object));

            if (parameterType == typeof(Func<object?>))
            {
                var lambda = Expression.Lambda<Func<object>>(converted);
                return lambda;
            }

            var x = new[] { typeof(double), typeof(string), typeof(bool) };

            if (x.Contains(parameterType))
            {
                return Expression.Convert(expression, parameterType);
            }

            return converted;
        }
    }

    public Expression VisitGroupingExpr(Expr.Grouping expr) => Build(expr.Expression);

    public Expression VisitLiteralExpr(Expr.Literal expr) => Expression.Constant(expr.Value);

    public Expression VisitLogicalExpr(Expr.Logical expr)
    {
        var lhs = Expression.Convert(Build(expr.Left), typeof(bool));
        var rhs = Expression.Convert(Build(expr.Right), typeof(bool));

        return expr.Operator.TokenType switch
        {
            AND => Expression.AndAlso(lhs, rhs),
            OR => Expression.OrElse(lhs, rhs),
            _ => throw new ExpressionException("Logical operator not and/or")
        };
    }

    public Expression VisitUnaryExpr(Expr.Unary expr)
    {
        var rhs = Build(expr.Right);

        return expr.Operator.TokenType switch
        {
            BANG => Expression.Not(Expression.Convert(rhs, typeof(bool))),
            MINUS => Expression.Negate(Expression.Convert(rhs, typeof(double))),
            _ => throw new ExpressionException("Shouldn't be able to get here")
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

    public List<ParameterExpression> GetParameters() => [.. _parameters.Values];

    private static (Expression lhc, Expression rhc) Conversions<T>(Expression lhs, Expression rhs)
    {
        // if we are converting to uint and lhs or rhs are Parameters,
        // we need to convert the parameters to double first,
        // then convert to uint. This is because the parameters are of type object,
        // and we cannot convert directly from object to uint.
        if (typeof(T) == typeof(uint))
        {
            if (lhs.NodeType == ExpressionType.Parameter)
            {
                lhs = Expression.Convert(lhs, typeof(double));
            }

            if(rhs.NodeType == ExpressionType.Parameter)
            {
                rhs = Expression.Convert(rhs, typeof(double));
            }
        }

        return (Expression.Convert(lhs, typeof(T)), 
                Expression.Convert(rhs, typeof(T)));
    }
}

