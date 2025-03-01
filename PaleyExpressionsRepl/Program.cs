
for(;;)
{
    Console.Write("> ");

    var command = Console.ReadLine();

    PaleyExpressions.Runner.Run(command);
}