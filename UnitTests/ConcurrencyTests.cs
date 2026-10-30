using PaleyExpressions;
using PaleyExpressions.Runners;

namespace UnitTests;

public class ConcurrencyTests
{
    [Fact]
    public async Task SameRunnerInstance_IsSafeForConcurrentEvaluations()
    {
        // The barrier in pause forces both evaluations to overlap after reading x.
        ConcurrencyFunctions.Initialize(2);

        var astRunner = new AstRunner(
            "pause(x) + y",
            typeof(ConcurrencyFunctions));

        var expressionRunner = new ExpressionRunner(
            "pause(x) + y",
            typeof(ConcurrencyFunctions));

        var astGenericRunner = new AstRunner<ConcurrencyContext>(
            "pause(x) + y",
            typeof(ConcurrencyFunctions));

        var expressionGenericRunner = new ExpressionRunner<ConcurrencyContext>(
            "pause(x) + y",
            typeof(ConcurrencyFunctions));

        var astResults = await RunConcurrently(
            astRunner.Interpret,
            new Dictionary<string, object?> { ["x"] = 1d, ["y"] = 10d },
            new Dictionary<string, object?> { ["x"] = 2d, ["y"] = 20d });

        // Reset the barrier before repeating the same concurrency check for the
        // expression-tree runner.
        ConcurrencyFunctions.Initialize(2);

        var expressionResults = await RunConcurrently(
            expressionRunner.Interpret,
            new Dictionary<string, object?> { ["x"] = 1d, ["y"] = 10d },
            new Dictionary<string, object?> { ["x"] = 2d, ["y"] = 20d });

        ConcurrencyFunctions.Initialize(2);

        var astGenericResults = await RunConcurrently(
            astGenericRunner.Interpret,
            new ConcurrencyContext { x = 1d, y = 10d },
            new ConcurrencyContext { x = 2d, y = 20d });

        ConcurrencyFunctions.Initialize(2);

        var expressionGenericResults = await RunConcurrently(
            expressionGenericRunner.Interpret,
            new ConcurrencyContext { x = 1d, y = 10d },
            new ConcurrencyContext { x = 2d, y = 20d });

        // Each evaluation must use its own variables or context, regardless of
        // scheduling. The sorted results make the assertion independent of which
        // task completes first.
        Assert.Equal([11d, 22d], astResults.Order());
        Assert.Equal([11d, 22d], expressionResults.Order());
        Assert.Equal([11d, 22d], astGenericResults.Order());
        Assert.Equal([11d, 22d], expressionGenericResults.Order());
    }

    private static async Task<double[]> RunConcurrently(
        Func<Dictionary<string, object?>, object?> interpret,
        params Dictionary<string, object?>[] variables)
    {
        // Start one task per variable set and wait until all evaluations finish.
        var tasks = variables
            .Select(values => Task.Run(() => (double)interpret(values)!))
            .ToArray();

        return await Task.WhenAll(tasks);
    }

    private static async Task<double[]> RunConcurrently<T>(
        Func<T, object?> interpret,
        params T[] contexts)
    {
        // The generic overload mirrors the dictionary test while passing typed
        // contexts, which exercises the runners' context isolation as well.
        var tasks = contexts
            .Select(context => Task.Run(() => (double)interpret(context)!))
            .ToArray();

        return await Task.WhenAll(tasks);
    }
}

public static class ConcurrencyFunctions
{
    private static Barrier _barrier = null!;

    public static void Initialize(int participants)
    {
        // A fresh barrier is required for each pair of concurrent evaluations.
        _barrier = new Barrier(participants);
    }

    [Function("pause")]
    public static double Pause(double value)
    {
        // Waiting here exposes runners that store per-call state in shared fields.
        _barrier.SignalAndWait(TimeSpan.FromSeconds(5));
        return value;
    }
}

public class ConcurrencyContext
{
    public double x;
    public double y;
}
