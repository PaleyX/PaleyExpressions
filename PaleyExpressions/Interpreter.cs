using static PaleyExpressions.TokenType;

namespace PaleyExpressions
{
    internal class Interpreter : Expr.IVisitor<object?>
    {
        public object? Interpret(Expr expression)
        {
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
            return "Hello";
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
            }
            // Unreachable.
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            //Object callee = evaluate(expr.callee);

            //List<Object> arguments = new ArrayList<>();
            //for (Expr argument : expr.arguments)
            //{
            //    arguments.add(evaluate(argument));
            //}

            //LoxCallable function = (LoxCallable)callee;
            //return function.call(this, arguments);

            return "hello";
        }

        private object? Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

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
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            return a.Equals(b);
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
}
