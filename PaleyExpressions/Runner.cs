namespace PaleyExpressions
{
    public static class Runner
    {
        public static object? Run(string source)
        {
            try
            { 
                var tokens = new Scanner(source).ScanTokens();
                var parser = new Parser(tokens);
                var expression = parser.Parse();

                Console.WriteLine(new AstPrinter().Print(expression));

                var result = new Interpreter().Interpret(expression);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }
    }
}
