using System.Text;

namespace PaleyExpressions.Visitors;

internal class AstPrinter : Expr.IVisitor<string> 
{
    public string Print(Expr expr) => expr.Accept(this);

    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitCallExpr(Expr.Call expr)
    {
        return Parenthesize2("fn", expr.Callee, expr.Arguments);
    }

    public string VisitVariableExpr(Expr.Variable expr)
    {
        return expr.Name.Lexeme;
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value == null ? "nil" : expr.Value.ToString();
    }

    public string VisitLogicalExpr(Expr.Logical expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var builder = new StringBuilder();

        builder.Append('(').Append(name);

        foreach (var expr in exprs)
        {
            builder.Append(' ');
            builder.Append(expr.Accept(this));
        }
        builder.Append(')');

        return builder.ToString();
    }

    private string Parenthesize2(string name, params object?[] parts)
    {
        var builder = new StringBuilder();

        builder.Append('(').Append(name);
        Transform(builder, parts);
        builder.Append(')');

        return builder.ToString();
    }

    private void Transform(StringBuilder builder, params object?[] parts)
    {
        foreach (var part in parts)
        {
            builder.Append(' ');

            switch (part)
            {
                case Expr expr:
                    builder.Append(expr.Accept(this));
                    break;
                case Token token:
                    builder.Append(token.Lexeme);
                    break;
                case List<Expr> list:
                    Transform(builder, [.. list]);
                    break;
                default:
                    builder.Append(part);
                    break;
            }
        }
    }
}