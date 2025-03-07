namespace PaleyExpressions;

public class ScannerException(string message) : Exception(message)
{
    internal static ScannerException TokenMessage(Token token, string message)
    {
        string? text;

        if (token.TokenType == TokenType.EOF)
        {
            text = $"{message}: at end";
        }
        else
        {
            text = $"{message}: at '{token.Lexeme}'";
        }

        return new ScannerException(text);
    }
}