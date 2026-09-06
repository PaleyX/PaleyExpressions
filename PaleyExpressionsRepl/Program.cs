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
                Console.WriteLine(result);
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
    var ast = new AstRunner(expression, typeof(Functions)).Interpret(_variables);
    var expr = new ExpressionRunner(expression, typeof(Functions)).Interpret(_variables);

    Debug.Assert((ast == null && expr == null) || ast!.Equals(expr));

    return ast;
}
partial class Program
{
    [GeneratedRegex("^[a-zA-Z_$][a-zA-Z_$0-9]*$")]
    private static partial Regex IdentifierRegex();
}