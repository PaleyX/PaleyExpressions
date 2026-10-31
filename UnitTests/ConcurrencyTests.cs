using PaleyExpressions;
using PaleyExpressions.Runners;

namespace UnitTests;

public class ConcurrencyTests
{
    // Verifies that all runner variants isolate per-evaluation state when two
    // calls are forced to overlap inside a user-defined function.
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

    // Verifies that lazy parsing or compilation is initialized safely when 64
    // calls reach a previously unused runner at the same time.
    [Fact]
    public async Task FirstUse_IsSafeUnderHighContention()
    {
        const int count = 64;
        var astRunner = new AstRunner("x + 1");
        var expressionRunner = new ExpressionRunner("x + 1");
        var astGenericRunner = new AstRunner<ConcurrencyContext>("x + 1");
        var expressionGenericRunner = new ExpressionRunner<ConcurrencyContext>("x + 1");

        var astResults = await RunFirstUseConcurrently(
            count,
            i => astRunner.Interpret(new Dictionary<string, object?> { ["x"] = (double)i }));
        var expressionResults = await RunFirstUseConcurrently(
            count,
            i => expressionRunner.Interpret(new Dictionary<string, object?> { ["x"] = (double)i }));
        var astGenericResults = await RunFirstUseConcurrently(
            count,
            i => astGenericRunner.Interpret(new ConcurrencyContext { x = i }));
        var expressionGenericResults = await RunFirstUseConcurrently(
            count,
            i => expressionGenericRunner.Interpret(new ConcurrencyContext { x = i }));

        Assert.Equal(Enumerable.Range(1, count).Select(i => (double)i), astResults.Cast<double>().Order());
        Assert.Equal(Enumerable.Range(1, count).Select(i => (double)i), expressionResults.Cast<double>().Order());
        Assert.Equal(Enumerable.Range(1, count).Select(i => (double)i), astGenericResults.Cast<double>().Order());
        Assert.Equal(Enumerable.Range(1, count).Select(i => (double)i), expressionGenericResults.Cast<double>().Order());
    }

    // Verifies that many concurrent evaluations do not mix dictionary values or
    // typed contexts by checking unique inputs against their expected results.
    [Fact]
    public async Task RepeatedConcurrentEvaluations_KeepInputsIsolated()
    {
        const int count = 256;
        var astRunner = new AstRunner("x * 2 + y");
        var expressionRunner = new ExpressionRunner("x * 2 + y");
        var astGenericRunner = new AstRunner<ConcurrencyContext>("x * 2 + y");
        var expressionGenericRunner = new ExpressionRunner<ConcurrencyContext>("x * 2 + y");

        var astResults = await RunIndexedConcurrently(
            count,
            i => astRunner.Interpret(new Dictionary<string, object?>
            {
                ["x"] = (double)i,
                ["y"] = (double)(i + 1)
            }));
        var expressionResults = await RunIndexedConcurrently(
            count,
            i => expressionRunner.Interpret(new Dictionary<string, object?>
            {
                ["x"] = (double)i,
                ["y"] = (double)(i + 1)
            }));
        var astGenericResults = await RunIndexedConcurrently(
            count,
            i => astGenericRunner.Interpret(new ConcurrencyContext { x = i, y = i + 1 }));
        var expressionGenericResults = await RunIndexedConcurrently(
            count,
            i => expressionGenericRunner.Interpret(new ConcurrencyContext { x = i, y = i + 1 }));

        var expected = Enumerable.Range(0, count).Select(i => (double)(3 * i + 1));
        Assert.Equal(expected, astResults.Cast<double>().Order());
        Assert.Equal(expected, expressionResults.Cast<double>().Order());
        Assert.Equal(expected, astGenericResults.Cast<double>().Order());
        Assert.Equal(expected, expressionGenericResults.Cast<double>().Order());
    }

    // Verifies that concurrent calls consistently report missing variables and
    // do not corrupt the runner after an evaluation fails.
    [Fact]
    public async Task MissingVariables_AreReportedSafelyUnderConcurrency()
    {
        var astRunner = new AstRunner("missing + 1");
        var expressionRunner = new ExpressionRunner("missing + 1");
        var astGenericRunner = new AstRunner<ConcurrencyContext>("missing + 1");
        var expressionGenericRunner = new ExpressionRunner<ConcurrencyContext>("missing + 1");

        await AssertConcurrentFailures(32, () => astRunner.Interpret());
        await AssertConcurrentFailures(32, () => expressionRunner.Interpret());
        await AssertConcurrentFailures(32, () => astGenericRunner.Interpret(new ConcurrencyContext()));
        await AssertConcurrentFailures(32, () => expressionGenericRunner.Interpret(new ConcurrencyContext()));
    }

    // Verifies that concurrent AST and expression-tree evaluations preserve
    // lazy branch semantics by ensuring the unselected function never runs.
    [Fact]
    public async Task LazyBranches_DoNotEvaluateUnselectedFunctionsConcurrently()
    {
        const string expression = "iif(true, 42, fail())";
        var astRunner = new AstRunner(expression, typeof(LazyBranchFunctions));
        var expressionRunner = new ExpressionRunner(expression, typeof(LazyBranchFunctions));

        var astResults = await RunIndexedConcurrently(64, _ => astRunner.Interpret());
        var expressionResults = await RunIndexedConcurrently(64, _ => expressionRunner.Interpret());

        Assert.All(astResults, result => Assert.Equal(42d, result));
        Assert.All(expressionResults, result => Assert.Equal(42d, result));
    }

    // Verifies that typed runners can concurrently create and use property
    // getters without sharing mutable per-evaluation state.
    [Fact]
    public async Task GenericRunners_SafelyReadPropertiesConcurrently()
    {
        var astRunner = new AstRunner<PropertyContext>("number * 2");
        var expressionRunner = new ExpressionRunner<PropertyContext>("number * 2");

        var astResults = await RunIndexedConcurrently(
            64,
            i => astRunner.Interpret(new PropertyContext(i)));
        var expressionResults = await RunIndexedConcurrently(
            64,
            i => expressionRunner.Interpret(new PropertyContext(i)));

        var expected = Enumerable.Range(0, 64).Select(i => (double)(i * 2));
        Assert.Equal(expected, astResults.Cast<double>().Order());
        Assert.Equal(expected, expressionResults.Cast<double>().Order());
    }

    // Verifies that a runner remains safe when it is reused for multiple
    // separate concurrent evaluation batches.
    [Fact]
    public async Task ReusingRunnerAcrossConcurrentBatches_RemainsSafe()
    {
        var astRunner = new AstRunner("x + 1");
        var expressionRunner = new ExpressionRunner("x + 1");
        var astGenericRunner = new AstRunner<ConcurrencyContext>("x + 1");
        var expressionGenericRunner = new ExpressionRunner<ConcurrencyContext>("x + 1");

        await AssertBatch(astRunner.Interpret, 0, 64);
        await AssertBatch(astRunner.Interpret, 100, 64);
        await AssertBatch(expressionRunner.Interpret, 0, 64);
        await AssertBatch(expressionRunner.Interpret, 100, 64);
        await AssertGenericBatch(astGenericRunner.Interpret, 0, 64);
        await AssertGenericBatch(astGenericRunner.Interpret, 100, 64);
        await AssertGenericBatch(expressionGenericRunner.Interpret, 0, 64);
        await AssertGenericBatch(expressionGenericRunner.Interpret, 100, 64);
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

    private static async Task<object?[]> RunFirstUseConcurrently(
        int count,
        Func<int, object?> interpret)
    {
        using var start = new Barrier(count + 1);
        var tasks = Enumerable.Range(0, count)
            .Select(i => Task.Run(() =>
            {
                start.SignalAndWait(TimeSpan.FromSeconds(5));
                return interpret(i);
            }))
            .ToArray();

        start.SignalAndWait(TimeSpan.FromSeconds(5));
        return await Task.WhenAll(tasks);
    }

    private static async Task<object?[]> RunIndexedConcurrently(
        int count,
        Func<int, object?> interpret)
    {
        var tasks = Enumerable.Range(0, count)
            .Select(i => Task.Run(() => interpret(i)))
            .ToArray();

        return await Task.WhenAll(tasks);
    }

    private static async Task AssertConcurrentFailures(int count, Func<object?> interpret)
    {
        var tasks = Enumerable.Range(0, count)
            .Select(_ => Task.Run(() => Assert.Throws<ExpressionException>(interpret)))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    private static async Task AssertBatch(
        Func<Dictionary<string, object?>, object?> interpret,
        int start,
        int count)
    {
        var results = await RunIndexedConcurrently(
            count,
            i => interpret(new Dictionary<string, object?> { ["x"] = (double)(start + i) }));

        Assert.Equal(
            Enumerable.Range(start + 1, count).Select(i => (double)i),
            results.Cast<double>().Order());
    }

    private static async Task AssertGenericBatch(
        Func<ConcurrencyContext, object?> interpret,
        int start,
        int count)
    {
        var results = await RunIndexedConcurrently(
            count,
            i => interpret(new ConcurrencyContext { x = start + i }));

        Assert.Equal(
            Enumerable.Range(start + 1, count).Select(i => (double)i),
            results.Cast<double>().Order());
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

public static class LazyBranchFunctions
{
    [Function("fail")]
    public static double Fail() => throw new InvalidOperationException("The unselected branch was evaluated.");
}

public class PropertyContext(double number)
{
    public double number { get; } = number;
}
