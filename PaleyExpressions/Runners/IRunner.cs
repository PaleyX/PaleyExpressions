namespace PaleyExpressions.Runners;

public interface IRunner
{
    object? Interpret(Dictionary<string, object?>? variables = null);
}