namespace PaleyExpressions.Visitors;

internal static class Helpers
{
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
}
