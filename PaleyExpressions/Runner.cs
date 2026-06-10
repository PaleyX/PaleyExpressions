using System.Linq.Expressions;
using System.Reflection;

namespace PaleyExpressions;

public static class Runner
{
    public static object? RunAst(string source, Dictionary<string, object?>? variables = null, Type? functions = null)
    { 
        var astRunner = new AstRunner(source, functions);
        astRunner.GetAst();
            
        var result = astRunner.Interpret(variables);

        return result;
    }

    public static object? RunExpression(string source, Dictionary<string, object?>? variables = null, Type? functions = null)
    {
        try
        {
            var expression = Parse(source, functions);

            var builder = new ExpressionBuilder();
            var built = builder.Build(expression);
            var compiled = Expression.Lambda(built, builder.GetParameters()).Compile();

            var args = GetExpressionArgs(builder.GetParameters().Select(p => p.Name), variables);

            var result = compiled.DynamicInvoke([.. args]);

            return result;
        }
        // DynamicInvoke wraps exceptions in a TargetInvocationException,
        // so we need to unwrap it to get the original exception.
        catch (TargetInvocationException tie)
        {
            if (tie.InnerException != null)
            {
                throw tie.InnerException;
            }

            throw;
        }
    }

    internal static List<object?> GetExpressionArgs(IEnumerable<string> parameters, Dictionary<string, object?>? variables)
    {
        var args = new List<object?>();

        foreach (var variable in parameters)
        {
            if (variables != null && variables.TryGetValue(variable, out var value))
            {
                args.Add(value);
            }
            else
            {
                throw new ScannerException($"Unknown variable '{variable}'");
            }
        }

        return args;
    }

    private static Expr Parse(string source, Type? functions)
    {
        var tokens = new Scanner(source).ScanTokens();
        var parser = new Parser(tokens, functions);

        return parser.Parse();
    }
}