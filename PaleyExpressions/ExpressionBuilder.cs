//#define USEPROOF

using System.Linq.Expressions;
using static PaleyExpressions.TokenType;

namespace PaleyExpressions;

internal class ExpressionBuilder(Dictionary<string, object?>? variables = null) : Expr.IVisitor<Expression>
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
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return Expression.GreaterThan(lhs, rhs);
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return Expression.GreaterThanOrEqual(lhs, rhs);
            case LESS:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return Expression.LessThan(lhs, rhs);
            case LESS_EQUAL:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return Expression.LessThanOrEqual(lhs, rhs);
            case MINUS:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return Expression.Subtract(lhs, rhs);
            case BANG_EQUAL:
                return Expression.NotEqual(lhs, rhs);
            case EQUAL_EQUAL:
                return Expression.Equal(lhs, rhs);
            case PLUS:
                if (lhs.Type == typeof(double) && rhs.Type == typeof(double))
                {
                    return Expression.Add(lhs, rhs);
                }

                if (lhs.Type == typeof(string) && rhs.Type == typeof(string))
                {
                    return Expression.Add(lhs, rhs, typeof(string).GetMethod("Concat", [typeof(string), typeof(string)]));
                }

                throw ScannerException.TokenMessage(expr.Operator, "Operands must be two numbers or two strings");
            case SLASH:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return Expression.Divide(lhs, rhs);
            case STAR:
                CheckNumberOperands(expr.Operator, lhs, rhs);
                return Expression.Multiply(lhs, rhs);

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
        if (variables != null && variables.TryGetValue(expr.Name.Lexeme, out var value))
        {
            if (_parameters.TryGetValue(expr.Name.Lexeme, out var p))
            {
                return p;
            }

            var type = value switch
            {
                string => typeof(string),
                double => typeof(double),
                bool => typeof(bool),
                _ => throw new ScannerException($"Unsupported variable type: '{expr.Name.Lexeme}'")
            };

            var parameter = Expression.Parameter(type, expr.Name.Lexeme);

            _parameters.Add(expr.Name.Lexeme, parameter);

            return parameter;
        }

        throw new ScannerException($"Unknown variable '{expr.Name.Lexeme}'");
    }

    public List<ParameterExpression> GetParameters() => _parameters.Values.ToList();

    private static void CheckNumberOperands(Token token, Expression left, Expression right)
    {
        if (left.Type == typeof(double) && right.Type == typeof(double))
        {
            return;
        }

        throw ScannerException.TokenMessage(token, "Operands must be numbers");
    }
}