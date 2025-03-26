//#define USEPROOF

using System.Diagnostics;
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
                var convert = Conversions<double>(lhs, rhs);
                return Expression.GreaterThan(convert.lhc, convert.rhc);
            }
            case GREATER_EQUAL:
            {
                var convert = Conversions<double>(lhs, rhs);
                return Expression.GreaterThanOrEqual(convert.lhc, convert.rhc);
            }
            case LESS:
            {
                var convert = Conversions<double>(lhs, rhs);
                return Expression.LessThan(convert.lhc, convert.rhc);
            }
            case LESS_EQUAL:
            {
                var convert = Conversions<double>(lhs, rhs);
                return Expression.LessThanOrEqual(convert.lhc, convert.rhc);
            }
            case MINUS:
            {
                var convert = Conversions<double>(lhs, rhs);
                return Expression.Subtract(convert.lhc, convert.rhc);
            }
            case BANG_EQUAL:
            {
                var equals = typeof(object).GetMethod("Equals", [typeof(object), typeof(object)]);
                var convert = Conversions<object>(lhs, rhs);
                return Expression.Not(Expression.Call(equals!, [convert.lhc, convert.rhc]));
            }
            case EQUAL_EQUAL:
            {
                var equals = typeof(object).GetMethod("Equals", [typeof(object), typeof(object)]);
                var convert = Conversions<object>(lhs, rhs);
                return Expression.Call(equals!, [convert.lhc, convert.rhc]);
            }
            case PLUS:
            {
                var plus = typeof(Helpers).GetMethod("Plus", BindingFlags.NonPublic | BindingFlags.Static);
                var convert = Conversions<object>(lhs, rhs);
                return Expression.Call(plus!, [convert.lhc, convert.rhc]);
            }
            case SLASH:
            {
                var convert = Conversions<double>(lhs, rhs);
                return Expression.Divide(convert.lhc, convert.rhc);
            }
            case STAR:
            {
                var convert = Conversions<double>(lhs, rhs);
                return Expression.Multiply(convert.lhc, convert.rhc);
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

            if (parameters[item.index].ParameterType == typeof(Func<object?>))
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
                var lambda = Expression.Lambda<Func<object>>(converted/*, GetParameters()*/);
                args.Add(lambda);
            }
            else
            {
                args.Add(built);
            }
        }

        return Expression.Call(expr.Function, [.. args]);


        throw new NotImplementedException();
    }

    public Expression VisitGroupingExpr(Expr.Grouping expr)
    {
        return Build(expr.Expression);
    }

    public Expression VisitLiteralExpr(Expr.Literal expr)
    {
        return Expression.Constant(expr.Value);
    }

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
            BANG => Expression.Not(rhs),
            MINUS => Expression.Negate(rhs),
            _ => throw new ScannerException("Shouldn't be able to get here")
        };
    }

    public Expression VisitVariableExpr(Expr.Variable expr)
    {
        // can be turned into a simple Contains
        //if (variables != null && variables.TryGetValue(expr.Name.Lexeme, out var value))
        //{
            if (_parameters.TryGetValue(expr.Name.Lexeme, out var p))
            {
                return p;
            }

            //var type = value switch
            //{
            //    string => typeof(string),
            //    double => typeof(double),
            //    bool => typeof(bool),
            //    _ => throw new ScannerException($"Unsupported variable type: '{expr.Name.Lexeme}'")
            //};

            var parameter = Expression.Parameter(typeof(object), expr.Name.Lexeme);

            _parameters.Add(expr.Name.Lexeme, parameter);

            return parameter;
        //}

        //throw new ScannerException($"Unknown variable '{expr.Name.Lexeme}'");
    }

    public List<ParameterExpression> GetParameters() => _parameters.Values.ToList();

    //private static void CheckNumberOperands(Token token, Expression left, Expression right)
    //{
    //    if (left.Type == typeof(double) && right.Type == typeof(double))
    //    {
    //        return;
    //    }

    //    throw ScannerException.TokenMessage(token, "Operands must be numbers");
    //}

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
        if (lhs is double d1 && rhs is double d2)
        {
            return d1 + d2;
        }

        if (lhs is string s1 && rhs is string s2)
        {
            return s1 + s2;
        }

        throw new ScannerException("Operands must be two numbers or two strings");
    }
}