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
            throw new ExpressionException($"Unknown function '{name}'");
        }

        if (!function.IsStatic)
        {
            throw new ExpressionException("Callable function '{name}' must be static");
        }

        if (function.IsGenericMethod)
        {
            throw new ExpressionException("Callable function '{name}' cannot be generic");
        }

        var isParams = function.GetParameters().LastOrDefault()?.IsDefined(typeof(ParamArrayAttribute), false) ?? false;

        if (isParams)
        {
            var paramType = function.GetParameters().LastOrDefault().ParameterType.GetElementType();
            if (!(paramType == typeof(object) || paramType == typeof(Func<object>)))
            {
                throw new ExpressionException($"{function.Name}: params type must be object or Func<object>");
            }

            if (args.Count() < function.GetParameters().Length - 1)
            {
                throw new ExpressionException($"Function '{name}': argument count mismatch");
            }
        }
        else
        {
            if (function.GetParameters().Length != args.Count())
            {
                throw new ExpressionException($"Function '{name}' expected {function.GetParameters().Length} argument(s) but got {args.Count()}");
            }
        }

        return function;
    }
}