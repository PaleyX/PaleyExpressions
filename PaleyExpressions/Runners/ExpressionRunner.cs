using System.Linq.Expressions;
using System.Reflection;
using PaleyExpressions.Visitors;

namespace PaleyExpressions.Runners;

public class ExpressionRunner(string source, Type? functions = null) : IRunner
{
    private Func<object?[], object?>? _invoker;
    private List<ParameterExpression>? _parameters;

    public object? Interpret(Dictionary<string, object?>? variables = null)
    {
        try
        {
            if (_invoker == null)
            {
                var expression = Helpers.Parse(source, functions);
                var builder = new ExpressionBuilder();
                var built = builder.Build(expression);
                _parameters = builder.GetParameters();

                // Build a fast object[] -> object invoker that avoids DynamicInvoke
                var argsParam = Expression.Parameter(typeof(object[]), "args");
                var replacements = new Dictionary<ParameterExpression, Expression>();

                for (var i = 0; i < _parameters.Count; i++)
                {
                    var p = _parameters[i];
                    var indexed = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                    replacements[p] = Expression.Convert(indexed, p.Type);
                }

                var replacer = new ParameterReplacer(replacements);
                var bodyWithReplacedParams = replacer.Visit(built);
                var bodyAsObject = Expression.Convert(bodyWithReplacedParams, typeof(object));
                var lambda = Expression.Lambda<Func<object?[], object?>>(bodyAsObject, argsParam);
                _invoker = lambda.Compile();
            }

            var args = GetExpressionArgs(_parameters!, variables);
            return _invoker(args);
        }
        catch (TargetInvocationException tie)
        {
            if (tie.InnerException != null) throw tie.InnerException;
            throw;
        }
    }

    private object?[] _args = [];

    private object?[] GetExpressionArgs(IReadOnlyList<ParameterExpression> parameters, Dictionary<string, object?>? variables)
    {
        if (parameters.Count == 0)
        {
            return [];
        }

        if(_args.Length == 0)
        {
            _args = new object?[parameters.Count];
        }

        for (var i = 0; i < parameters.Count; i++)
        {
            var name = parameters[i].Name;
            if (variables != null && variables.TryGetValue(name, out var value))
            {
                _args[i] = value;
            }
            else
            {
                throw new ExpressionException($"Unknown variable '{name}'");
            }
        }

        return _args;
    }

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
    }
}
