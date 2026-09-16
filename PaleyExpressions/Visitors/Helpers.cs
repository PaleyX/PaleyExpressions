using System.Linq.Expressions;
using System.Reflection;

namespace PaleyExpressions.Visitors;

internal static class Helpers
{
    internal static Expression GetPlus(Expression lhs, Expression rhs)
    {
        if (TryFoldable<double>(lhs, rhs, out var folds))
        {
            return Expression.Constant(folds.lhs + folds.rhs, typeof(double));
        }

        if (lhs.Type == typeof(double) && rhs.Type == typeof(double))
        {
            return Expression.Add(lhs, rhs);
        }

        if (lhs.Type == typeof(string) && rhs.Type == typeof(string))
        {
            var concat = typeof(string).GetMethod("Concat", [typeof(string), typeof(string)]);
            return Expression.Call(concat!, lhs, rhs);
        }

        // Fallback for Parameter etc.
        var plus = typeof(Helpers).GetMethod("Plus", BindingFlags.NonPublic | BindingFlags.Static);
        var (lhc, rhc) = Conversions<object>(lhs, rhs);
        return Expression.Call(plus!, lhc, rhc);
    }

    internal static object Plus(object? lhs, object? rhs)
    {
        return lhs switch
        {
            double d1 when rhs is double d2 => d1 + d2,
            string s1 when rhs is string s2 => s1 + s2,
            _ => throw new ExpressionException("Operands must be two numbers or two strings")
        };
    }

    internal static object LeftShift(object? lhs, object? rhs)
    {
        return lhs switch
        {
            double d1 when rhs is double d2 => (double)((int)d1 << (int)d2),
            string s1 when rhs is double d3 => s1[(int)d3..],
            _ => throw new ExpressionException("Operands must be 2 numbers or a string and a number")
        };
    }

    internal static object RightShift(object? lhs, object? rhs)
    {
        return lhs switch
        {
            double d1 when rhs is double d2 => (double)((int)d1 >> (int)d2),
            string s1 when rhs is double d3 => s1[..^(int)d3],
            _ => throw new ExpressionException("Operands must be 2 numbers or a string and a number")
        };
    }

    internal static (Expression lhc, Expression rhc) Conversions<T>(Expression lhs, Expression rhs)
    {
        if (lhs.Type != typeof(T))
        {
            lhs = Convert<T>(lhs);
        }

        if (rhs.Type != typeof(T))
        {
            rhs = Convert<T>(rhs);
        }

        return (lhs, rhs);
    }

    internal static Expression Convert<T>(Expression convert)
    {
        // if we are converting to uint and 'convert' is a Parameter,
        // we need to convert the parameter to double first,
        // then convert to uint. This is because the parameter is of type object,
        // and we cannot convert directly from object to uint.

        if (typeof(T) == typeof(uint))
        {
            if (convert.NodeType == ExpressionType.Parameter)
            {
                convert = Convert<double>(convert);
            }
        }

        if (convert.Type == typeof(T))
        {
            return convert;
        }

        return Expression.Convert(convert, typeof(T));
    }

    internal static Expression Convert(Expression expression, Type targetType)
    {
        if (expression.Type == targetType)
        {
            return expression;
        }
        return Expression.Convert(expression, targetType);
    }

    internal static bool TryFoldable<T>(Expression lhs, Expression rhs, out (T lhs, T rhs) result)
    {
        if (lhs.Type == typeof(T) &&
            rhs.Type == typeof(T) &&
            lhs.NodeType == ExpressionType.Constant &&
            rhs.NodeType == ExpressionType.Constant)
        {
            var lhsValue = (T)((ConstantExpression)lhs).Value!;
            var rhsValue = (T)((ConstantExpression)rhs).Value!;

            result = (lhsValue, rhsValue);
            return true;
        }

        result = (default!, default!);
        return false;
    }
}
