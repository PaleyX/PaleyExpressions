using System.Reflection;

namespace PaleyExpressions
{
    internal static class Tools
    {
        internal static MethodInfo? GetFunction(string name, IEnumerable<Expr> args)
        {
            var methods = typeof(Builtins).GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(FunctionAttribute), false).Length > 0)
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

            if (function.GetParameters().Length != args.Count())
            {
                throw new ScannerException($"Function '{name}' expected {function.GetParameters().Length} argument(s) but got {args.Count()}");
            }

            return null;
        }
    }
}
