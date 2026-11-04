using PaleyExpressions.Visitors.Exp;
using System.Linq.Expressions;
using System.Reflection;

namespace PaleyExpressions.Runners;

/// <summary>
/// The runner constructor for building an Expression Tree.
/// </summary>
/// <param name="source">the source expression</param>
/// <param name="functions">the Type of a class containing user defined functions</param>
public class ExpressionRunner(string source, Type? functions = null) : IRunner
{
    // Compilation is expensive, so the generated delegate is cached. Lazy<T>
    // makes first use safe when multiple threads initialize the same runner at
    // once, while keeping the steady-state path to a single cached-value read.
    private readonly Lazy<CompiledExpression> _compiled = new(
        () => Compile(source, functions),
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Interprets the expression with the given variables.
    /// </summary>
    /// <param name="variables">the variables to use in the evaluation</param>
    /// <returns>the result of evaluating the expression</returns>
    public object? Interpret(Dictionary<string, object?>? variables = null)
    {
        try
        {
            var compiled = _compiled.Value;

            // The argument array must belong to this invocation. Reusing one on
            // the runner would let concurrent calls overwrite values while the
            // compiled delegate is still reading them. A local array is preferred
            // over a lock because it preserves parallel execution.
            var args = GetExpressionArgs(compiled.Parameters, variables);
            return compiled.Invoker(args);
        }
        catch (TargetInvocationException tie)
        {
            if (tie.InnerException != null) throw tie.InnerException;
            throw;
        }
    }

    private static CompiledExpression Compile(string source, Type? functions)
    {
        var expression = Helpers.Parse(source, functions);
        var builder = new ExpressionBuilder();
        var built = builder.Build(expression);
        var parameters = builder.GetParameters();

        // Build a fast object[] -> object invoker that avoids DynamicInvoke.
        // This work runs once inside Lazy<T>; evaluations only invoke the cached
        // delegate and prepare their own arguments.
        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var replacements = new Dictionary<ParameterExpression, Expression>();

        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            var indexed = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            replacements[parameter] = Expression.Convert(indexed, parameter.Type);
        }

        var replacer = new ParameterReplacer(replacements);
        var bodyWithReplacedParams = replacer.Visit(built);
        var bodyAsObject = Expression.Convert(bodyWithReplacedParams, typeof(object));
        var lambda = Expression.Lambda<Func<object?[], object?>>(bodyAsObject, argsParam);

        return new CompiledExpression(lambda.Compile(), parameters);
    }

    private static object?[] GetExpressionArgs(
        IReadOnlyList<ParameterExpression> parameters,
        IReadOnlyDictionary<string, object?>? variables)
    {
        if (parameters.Count == 0)
        {
            return [];
        }

        // This allocation is intentional: it is the ownership boundary that
        // makes argument values independent between concurrent evaluations.
        var args = new object?[parameters.Count];

        for (var i = 0; i < parameters.Count; i++)
        {
            var name = parameters[i].Name!;
            if (variables != null && variables.TryGetValue(name, out var value))
            {
                args[i] = value;
            }
            else
            {
                throw new ExpressionException($"Unknown variable '{name}'");
            }
        }

        return args;
    }

    private sealed record CompiledExpression(
        Func<object?[], object?> Invoker,
        IReadOnlyList<ParameterExpression> Parameters);

    private sealed class ParameterReplacer(IReadOnlyDictionary<ParameterExpression, Expression> map) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (map.TryGetValue(node, out var replacement))
            {
                return replacement;
            }

            return base.VisitParameter(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            // C# conversion rules convert when converting bool to double convert
            // true to 1 and false to 0 - but this is not what we want in PaleyExpressions.
            // We want to throw an exception if the user tries to convert a boolean to a double.
            if (node.NodeType == ExpressionType.Convert && node.Type == typeof(double))
            {
                if (node.Operand.Type == typeof(bool))
                {
                    throw new ExpressionException("Cannot convert boolean to double");
                }
            }

            return base.VisitUnary(node);
        }
    }
}

public class ExpressionRunner<T>(string source, Type? functions = null) where T : class
{
    // The compiled delegate has no per-call mutable state. Lazy<T> prevents
    // duplicate or partially published compilation during concurrent first use.
    private readonly Lazy<Func<T, object?>> _compiled = new(
        () => Compile(source, functions),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public object? Interpret(T context)
    {
        return _compiled.Value(context);
    }

    private static Func<T, object?> Compile(string source, Type? functions)
    {
        var expression = Helpers.Parse(source, functions);
        var builder = new ExpressionBuilder<T>();
        var built = builder.Build(expression);
        var bodyAsObject = Expression.Convert(built, typeof(object));
        var lambda = Expression.Lambda<Func<T, object?>>(bodyAsObject, builder.Context);

        return lambda.Compile();
    }
}

