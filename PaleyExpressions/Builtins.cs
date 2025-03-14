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

    internal static List<Type> FunctionSources { get; } = [typeof(Builtins)];

    public static void AddFunctionsClass(Type type)
    {
        FunctionSources.Insert(0, type);
    }
}