// ReSharper disable UnusedMember.Global
namespace PaleyExpressions;

public static class Builtins
{
    [Function("abs")]
    public static double Abs(double d) => Math.Abs(d);

    [Function("upper")]
    public static string Upper(string s) => s.ToUpper();

    [Function("lower")]
    public static string Lower(string s) => s.ToLower();

    [Function("iif")]
    public static object? Iif(bool condition, Func<object?> ifTrue, Func<object?> ifFalse)
    {
        return condition ? ifTrue() : ifFalse();
    }

    [Function("cond")]
    public static object? Cond(params Func<object?>[] gort)
    {
        if (gort.Length < 2 || gort.Length % 2 != 0)
        {
            throw new ExpressionException("Cond must have arguments in multiples of 2");
        }

        for (int i = 0; i < gort.Length; i += 2)
        {
            if (gort[i]() is true)
            {
                return gort[i + 1]();
            }
        }

        return null;
    }

    internal static List<Type> FunctionSources { get; } = [typeof(Builtins)];

    public static void AddFunctionsClass(Type type)
    {
        FunctionSources.Insert(0, type);
    }
}