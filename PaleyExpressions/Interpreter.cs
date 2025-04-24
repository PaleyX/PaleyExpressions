using System.Diagnostics;
using System.Threading.Tasks.Sources;
using static PaleyExpressions.TokenType;

namespace PaleyExpressions;

internal class Interpreter() : Expr.IVisitor<object?>
{
    private Dictionary<string, object?>? _variables;

    public object? Interpret(Expr expression, Dictionary<string, object?>? variables) 
    {
        _variables = variables;

        var value = Evaluate(expression);
        return value;
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Operator.TokenType == OR) 
        {
            if (IsTruthy(left)) return left;
        } 
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        if (_variables != null && _variables.TryGetValue(expr.Name.Lexeme, out var value))
        {
            return value;
        }

        throw new ScannerException($"Unknown variable '{expr.Name.Lexeme}'");
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);

        switch (expr.Operator.TokenType) 
        {
            case BANG:
                return !IsTruthy(right);
            case MINUS:
                return -CheckNumberOperand(expr.Operator, right);
        }

        // Unreachable.
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Operator.TokenType)
        {
            case GREATER:
            {
                var (lhs, rhs) = CheckNumberOperands(expr.Operator, left, right);
                return lhs > rhs;
            }
            case GREATER_EQUAL:
            {
                var (lhs, rhs) = CheckNumberOperands(expr.Operator, left, right);
                return lhs >= rhs;
            }
            case LESS:
            {
                var (lhs, rhs) = CheckNumberOperands(expr.Operator, left, right);
                return lhs < rhs;
            }
            case LESS_EQUAL:
            {
                var (lhs, rhs) = CheckNumberOperands(expr.Operator, left, right);
                return lhs <= rhs;
            }
            case MINUS:
            {
                var (lhs, rhs) = CheckNumberOperands(expr.Operator, left, right);
                return lhs - rhs;
            }
            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
            case PLUS:
                if (left is double d1 && right is double d2)
                {
                    return d1 + d2;
                }

                if (left is string s1 && right is string s2)
                {
                    return s1 + s2;
                }

                throw ScannerException.TokenMessage(expr.Operator, "Operands must be two numbers or two strings");
            case SLASH:
            {
                var (lhs, rhs) = CheckNumberOperands(expr.Operator, left, right);
                return lhs / rhs;
            }
            case STAR:
            {
                var (lhs, rhs) = CheckNumberOperands(expr.Operator, left, right);
                return lhs * rhs;
            }
            case MOD:
            {
                var (lhs, rhs) = CheckNumberOperands(expr.Operator, left, right);
                return lhs % rhs;
            }
        }
        // Unreachable.
        return null;
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        var args = new List<object?>();
        var parameters = expr.Function.GetParameters();

        var last = parameters.LastOrDefault();
        var paramsAdded = false;

        foreach (var item in expr.Arguments.Select((value, index) => (value, index)))
        {
            var parameter = parameters[item.index];

            var isParams = parameter.IsDefined(typeof(ParamArrayAttribute), false);

            if(isParams)
            {
                paramsAdded = true;

                var paramType = parameter.ParameterType.GetElementType();

                var array = Array.CreateInstance(paramType, expr.Arguments.Count - item.index);
                for (int i = item.index; i < expr.Arguments.Count; i++)
                {
                    array.SetValue(GetParameter(paramType, expr.Arguments[i]), i - item.index);
                }
                args.Add(array);
                break;
            }

            args.Add(GetParameter(parameter.ParameterType, item.value));
        }

        // if function has a params but the call doesnt have any, add an empty array
        if (last != null && last.IsDefined(typeof(ParamArrayAttribute), false))
        {
            if (!paramsAdded)
            {
                var paramType = last.ParameterType.GetElementType();
                var array = Array.CreateInstance(paramType, 0);
                args.Add(array);
            }
        }

        return expr.Function.Invoke(null, [.. args])!;

        object? GetParameter(Type parameterType, Expr value)
        {
            return parameterType == typeof(Func<object?>)
                ? () => Evaluate(value)
                : Evaluate(value);
        }
    }

    private object? Evaluate(Expr expr) => expr.Accept(this);

    private static bool IsTruthy(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is bool value)
        {
            return value;
        }

        return true;
    }

    private static bool IsEqual(object? a, object? b)
    {
        return a switch
        {
            null when b == null => true,
            null => false,
            _ => a.Equals(b)
        };
    }

    private static double CheckNumberOperand(Token token, object? operand)
    {
        if (operand is double d)
        {
            return d;
        }

        throw ScannerException.TokenMessage(token, "Operand must be a number");
    }

    private static (double lhs, double rhs) CheckNumberOperands(Token token, object? left, object? right)
    {
        if(left is double d1 && right is double d2)
        {
            return (d1, d2);
        }

        throw ScannerException.TokenMessage(token, "Operands must be numbers");
    }
}