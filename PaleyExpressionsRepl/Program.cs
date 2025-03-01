
for(;;)
{
    Console.Write("> ");

    var command = Console.ReadLine();

    var result = PaleyExpressions.Runner.Run(command);
    Console.WriteLine(result);
}