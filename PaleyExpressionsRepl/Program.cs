using System.Diagnostics;
using System.Text.RegularExpressions;
using PaleyExpressionsRepl;

Dictionary<string, object?> _variables = new();

for (;;)
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
    var name = command.Substring(1, pos - 1);
    var expression = command[(pos + 1)..];

    if (!IsValidIdentifier(name))
    {
        Console.WriteLine("Invalid variable name: '{name}'");
        return;
    }

    var result = ProcessExpression(expression);

    _variables[name] = result;
}

bool IsValidIdentifier(string name)
{
    return IdentifierRegex().IsMatch(name);
}

object? ProcessExpression(string expression)
{  
    var ast = PaleyExpressions.Runner.RunAst(expression, _variables, typeof(Functions));
    var expr = PaleyExpressions.Runner.RunExpression(expression, _variables, typeof(Functions));

    Debug.Assert((ast == null && expr == null) || ast!.Equals(expr));

    return ast;
}
partial class Program
{
    [GeneratedRegex("^[a-zA-Z_$][a-zA-Z_$0-9]*$")]
    private static partial Regex IdentifierRegex();
}