namespace PaleyExpressions
{
    internal abstract class Expr
    {
        internal interface IVisitor<R>
        {
            //R VisitAssignExpr(Assign expr);
            R VisitBinaryExpr(Binary expr);
            //R VisitCallExpr(Call expr);
            //R VisitGetExpr(Get expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            //R VisitLogicalExpr(Logical expr);
            //R VisitSetExpr(Set expr);
            //R VisitSuperExpr(Super expr);
            //R VisitThisExpr(This expr);
            R VisitUnaryExpr(Unary expr);
            //R VisitVariableExpr(Variable expr);
        }

        // Nested Expr classes here...

        internal abstract R Accept<R>(IVisitor<R> visitor);

        internal class Binary(Expr left, Token op, Expr right) : Expr
        {
            internal Expr Left { get; } = left;
            internal Token Operator { get; } = op;
            internal Expr Right { get; } = right;

            internal override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBinaryExpr(this);
        }

        internal class Grouping(Expr expr) : Expr
        {
            internal Expr Expression { get; } = expr;

            internal override R Accept<R>(IVisitor<R> visitor) => visitor.VisitGroupingExpr(this);
        }

        internal class Literal(object? value) : Expr
        {
            internal object? Value { get; } = value;

            internal override R Accept<R>(IVisitor<R> visitor) => visitor.VisitLiteralExpr(this);
        }

        //internal class Logical(Expr left, Token op, Expr right) : Expr
        //{
        //    internal Expr Left { get; } = left;
        //    internal Token Operator { get; } = op;
        //    internal Expr Right { get; } = right;

        //    internal override R Accept<R>(IVisitor<R> visitor) => visitor.VisitLogicalExpr(this);
        //}

        internal class Unary(Token op, Expr right) : Expr
        {
            internal Token Operator { get; } = op;
            internal Expr Right { get; } = right;

            internal override R Accept<R>(IVisitor<R> visitor) => visitor.VisitUnaryExpr(this);
        }
    }
}
