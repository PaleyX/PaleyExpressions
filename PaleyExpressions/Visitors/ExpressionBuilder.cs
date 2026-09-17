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
                var (lhc, rhc) = Helpers.Conversions<double>(lhs, rhs);
                return Expression.GreaterThan(lhc, rhc);
            }
            case GREATER_EQUAL:
            {
                var (lhc, rhc) = Helpers.Conversions<double>(lhs, rhs);
                return Expression.GreaterThanOrEqual(lhc, rhc);
            }
            case LESS:
            {
                var (lhc, rhc) = Helpers.Conversions<double>(lhs, rhs);
                return Expression.LessThan(lhc, rhc);
            }
            case LESS_EQUAL:
            {
                var (lhc, rhc) = Helpers.Conversions<double>(lhs, rhs);
                return Expression.LessThanOrEqual(lhc, rhc);
            }
            case MINUS:
            {
                if (Helpers.TryFoldable<double>(lhs, rhs, out var folds))
                {
                    return Expression.Constant(folds.lhs - folds.rhs, typeof(double));
                }
                var (lhc, rhc) = Helpers.Conversions<double>(lhs, rhs);
                return Expression.Subtract(lhc, rhc);
            }
            case BANG_EQUAL:
            {
                var equals = typeof(object).GetMethod("Equals", [typeof(object), typeof(object)]);
                var (lhc, rhc) = Helpers.Conversions<object>(lhs, rhs);
                return Expression.Not(Expression.Call(equals!, lhc, rhc));
            }
            case EQUAL_EQUAL:
            {
                var equals = typeof(object).GetMethod("Equals", [typeof(object), typeof(object)]);
                var (lhc, rhc) = Helpers.Conversions<object>(lhs, rhs);
                return Expression.Call(equals!, lhc, rhc);
            }
            case PLUS:
            {
                return Helpers.GetPlus(lhs, rhs);
            }
            case LEFT_SHIFT:
            case RIGHT_SHIFT:
            {
                var proc = expr.Operator.TokenType == LEFT_SHIFT ? "LeftShift" : "RightShift";
                var shift = typeof(Helpers).GetMethod(proc, BindingFlags.NonPublic | BindingFlags.Static);
                var (lhc, rhc) = Helpers.Conversions<object>(lhs, rhs);
                return Expression.Call(shift!, lhc, rhc);
            }
            case BITWISE_AND:
            {
                var (lhc, rhc) = Helpers.Conversions<uint>(lhs, rhs);
                return Helpers.Convert<double>(Expression.And(lhc, rhc));
            }
            case BITWISE_OR:
            {
                var (lhc, rhc) = Helpers.Conversions<uint>(lhs, rhs);
                return Helpers.Convert<double>(Expression.Or(lhc, rhc));
            }
            case SLASH:
            {
                if (Helpers.TryFoldable<double>(lhs, rhs, out var folds))
                {
                    return Expression.Constant(folds.lhs / folds.rhs, typeof(double));
                }
                var (lhc, rhc) = Helpers.Conversions<double>(lhs, rhs);
                return Expression.Divide(lhc, rhc);
            }
            case STAR:
            {
                if (Helpers.TryFoldable<double>(lhs, rhs, out var folds))
                {
                    return Expression.Constant(folds.lhs * folds.rhs, typeof(double));
                }
                var (lhc, rhc) = Helpers.Conversions<double>(lhs, rhs);
                return Expression.Multiply(lhc, rhc);
            }
            case MOD:
            {
                if (Helpers.TryFoldable<double>(lhs, rhs, out var folds))
                {
                    return Expression.Constant(folds.lhs % folds.rhs, typeof(double));
                }
                var (lhc, rhc) = Helpers.Conversions<double>(lhs, rhs);
                return Expression.Modulo(lhc, rhc);
            }
        }

        throw new ExpressionException("Shouldn't be able to get here");
    }

    public Expression VisitCallExpr(Expr.Call expr)
    {
        var args = new List<Expression>();
        var parameters = expr.Function.GetParameters();

        // Special-case the Builtins.Iif to emit a conditional expression instead of
        // creating delegate thunks for lazy branches. This preserves lazy semantics
        // while avoiding delegate allocation and invocation overhead 
        if (expr.Function.DeclaringType == typeof(Builtins) && expr.Function.Name == nameof(Builtins.Iif))
        {
            if (expr.Arguments.Count != 3)
                throw new ExpressionException("iif requires exactly three arguments");

            var test = Helpers.Convert<bool>(Build(expr.Arguments[0]));
            var ifTrue = Helpers.Convert<object>(Build(expr.Arguments[1]));
            var ifFalse = Helpers.Convert<object>(Build(expr.Arguments[2]));

            return Expression.Condition(test, ifTrue, ifFalse);
        }

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

        // if function has a params but the call doesn't have any parameters,
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
            var converted = Helpers.Convert<object>(expression);

            if (parameterType == typeof(Func<object?>))
            {
                var lambda = Expression.Lambda<Func<object>>(converted);
                return lambda;
            }

            var x = new[] { typeof(double), typeof(string), typeof(bool) };

            if (x.Contains(parameterType))
            {
                return Helpers.Convert(expression, parameterType);
            }

            return converted;
        }
    }

    public Expression VisitGroupingExpr(Expr.Grouping expr) => Build(expr.Expression);

    public Expression VisitLiteralExpr(Expr.Literal expr) => Expression.Constant(expr.Value);

    public Expression VisitLogicalExpr(Expr.Logical expr)
    {
        var lhs = Helpers.Convert<bool>(Build(expr.Left));
        var rhs = Helpers.Convert<bool>(Build(expr.Right));

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
            BANG => Expression.Not(Helpers.Convert<bool>(rhs)),
            MINUS => Expression.Negate(Helpers.Convert<double>(rhs)),
            BITWISE_NOT => Helpers.Convert<double>(Expression.Not(Helpers.Convert<uint>(rhs))),
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
}

