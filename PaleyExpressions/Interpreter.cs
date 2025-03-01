using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PaleyExpressions.TokenType;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object? VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Operator.TokenType) 
            {
                case BANG:
                    return !IsTruthy(right);
                case MINUS:
                    return -(double)right;
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
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    return (double)left >= (double)right;
                case LESS:
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    return (double)left <= (double)right;
                case MINUS:
                    return (double)left - (double)right;
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
                    break;
                case SLASH:
                    return (double)left / (double)right;
                case STAR:
                    return (double)left * (double)right;
            }

            // Unreachable.
            return null;
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
    }
}
