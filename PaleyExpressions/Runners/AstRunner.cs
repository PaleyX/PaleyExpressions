using PaleyExpressions.Visitors.Ast;

namespace PaleyExpressions.Runners;

public class AstRunner(string source, Type? functions = null) : IRunner
{
    // The parsed AST is immutable after construction, so it can safely be shared
    // by all evaluations. Lazy<T> ensures that concurrent first calls parse only
    // once instead of racing on a nullable field.
    private readonly Lazy<Expr> _expression = new(
        () => Helpers.Parse(source, functions),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public object? Interpret(Dictionary<string, object?>? variables = null)
    {
        // The interpreter stores the current variables while walking the tree.
        // A new instance per call keeps that per-evaluation state isolated without
        // locking the whole evaluation and therefore preserves parallelism.
        var interpreter = new AstInterpreter();

        return interpreter.Interpret(_expression.Value, variables);
    }
}

public class AstRunner<T>(string source, Type? functions = null) where T : class
{
    // The AST is immutable and is the only state that should be shared between
    // concurrent calls. Lazy<T> provides safe, one-time initialization.
    private readonly Lazy<Expr> _expression = new(
        () => Helpers.Parse(source, functions),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public object? Interpret(T context)
    {
        // AstInterpreter<T> stores the current context and lazily builds member
        // getters. Keeping it per-call avoids sharing either mutable state or a
        // non-thread-safe getter dictionary between evaluations.
        var interpreter = new AstInterpreter<T>();

        return interpreter.Interpret(_expression.Value, context);
    }
}

