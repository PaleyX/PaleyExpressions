﻿namespace PaleyExpressions
{
    internal abstract class Expr
    {
        internal interface IVisitor<out T>
        {
            //R VisitAssignExpr(Assign expr);
            T VisitBinaryExpr(Binary expr);
            T VisitCallExpr(Call expr);
            //R VisitGetExpr(Get expr);
            T VisitGroupingExpr(Grouping expr);
            T VisitLiteralExpr(Literal expr);
            T VisitLogicalExpr(Logical expr);
            //R VisitSetExpr(Set expr);
            //R VisitSuperExpr(Super expr);
            //R VisitThisExpr(This expr);
            T VisitUnaryExpr(Unary expr);
            T VisitVariableExpr(Variable expr);
        }

        // Nested Expr classes here...

        internal abstract T Accept<T>(IVisitor<T> visitor);

        internal class Binary(Expr left, Token op, Expr right) : Expr
        {
            internal Expr Left { get; } = left;
            internal Token Operator { get; } = op;
            internal Expr Right { get; } = right;

            internal override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpr(this);
        }

        internal class Call(Expr callee, Token paren, List<Expr> arguments) : Expr
        {
            internal Expr Callee { get; } = callee;
            internal Token Paren { get; } = paren;
            internal List<Expr> Arguments { get; } = arguments;

            internal override T Accept<T>(IVisitor<T> visitor) => visitor.VisitCallExpr(this);
        }

        internal class Grouping(Expr expr) : Expr
        {
            internal Expr Expression { get; } = expr;

            internal override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGroupingExpr(this);
        }

        internal class Literal(object? value) : Expr
        {
            internal object? Value { get; } = value;

            internal override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpr(this);
        }

        internal class Logical(Expr left, Token op, Expr right) : Expr
        {
            internal Expr Left { get; } = left;
            internal Token Operator { get; } = op;
            internal Expr Right { get; } = right;

            internal override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLogicalExpr(this);
        }

        internal class Unary(Token op, Expr right) : Expr
        {
            internal Token Operator { get; } = op;
            internal Expr Right { get; } = right;

            internal override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpr(this);
        }

        internal class Variable(Token name) : Expr
        {
            internal Token Name { get; } = name;

            internal override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableExpr(this);
        }
    }
}
