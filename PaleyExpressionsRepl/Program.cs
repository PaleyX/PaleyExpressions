using System.Diagnostics;
using System.Text.RegularExpressions;
using PaleyExpressions.Runners;
using PaleyExpressionsRepl;

Dictionary<string, object?> _variables = new();

while(true)
{
    try
    {
        Console.Write("> ");

        var command = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(command))
        {
            break;
        }

        command = command.Trim();

        switch (command[..1])
        {
            case "$":
                ProcessVariable(command);
                break;
            default:
                var result = ProcessExpression(command);
                Console.WriteLine(result ?? "Nil");
                break;
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}

return;

void ProcessVariable(string command)
{
    var pos = command.IndexOf(' ');
    var name = command[1..pos];
    var expression = command[(pos + 1)..];

    if (!IsValidIdentifier(name))
    {
        Console.WriteLine("Invalid variable name: '{name}'");
        return;
    }

    var result = ProcessExpression(expression);

    _variables[name] = result;
}

static bool IsValidIdentifier(string name)
{
    return IdentifierRegex().IsMatch(name);
}

object? ProcessExpression(string expression)
{
    object? astResult = null;
    object? exprResult = null;

    try
    {
        astResult = new AstRunner(expression, typeof(Functions)).Interpret(_variables);
    }
    catch (Exception e)
    {
        Console.WriteLine($"AST Error: {e.Message}");
    }

    try
    {
        exprResult = new ExpressionRunner(expression, typeof(Functions)).Interpret(_variables);
    }
    catch (Exception e)
    {
        Console.WriteLine($"Expression Error: {e.Message}");
    }

    Debug.Assert((astResult == null && exprResult == null) || astResult!.Equals(exprResult));

    return astResult;
}
partial class Program
{
    [GeneratedRegex("^[a-zA-Z_$][a-zA-Z_$0-9]*$")]
    private static partial Regex IdentifierRegex();
}