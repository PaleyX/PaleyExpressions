using System.Reflection;

namespace PaleyExpressions;

internal static class Tools
{
    internal static MethodInfo GetFunction(string name, IEnumerable<Expr> args)
    {
        var methods = Builtins.FunctionSources
            .SelectMany(t => t.GetMethods()
            .Where(m => m.GetCustomAttributes(typeof(FunctionAttribute), false).Length > 0))
                .ToList();

        var function = methods.FirstOrDefault(m =>
        {
            var attr = m.GetCustomAttribute<FunctionAttribute>();
            return attr?.Name == name;
        });

        if (function == null)
        {
            throw new ScannerException($"Unknown function '{name}'");
        }

        if (!function.IsStatic)
        {
            throw new ScannerException("Callable function '{name}' must be static");
        }

        if (function.IsGenericMethod)
        {
            throw new ScannerException("Callable function '{name}' cannot be generic");
        }

        if (function.GetParameters().Length != args.Count())
        {
            throw new ScannerException($"Function '{name}' expected {function.GetParameters().Length} argument(s) but got {args.Count()}");
        }

        return function;
    }
}