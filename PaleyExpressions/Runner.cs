namespace PaleyExpressions;

public static class Runner
{
    public static object? Run(string source, Dictionary<string, object?>? variables = null, Type? functions = null)
    {
        try
        {
            var tokens = new Scanner(source).ScanTokens();
            var parser = new Parser(tokens, functions);
            var expression = parser.Parse();

            //Console.WriteLine(new AstPrinter().Print(expression));

            var result = new Interpreter(variables).Interpret(expression);

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return null;
    }
}