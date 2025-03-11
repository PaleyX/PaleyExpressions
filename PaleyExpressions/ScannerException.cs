namespace PaleyExpressions;

public class ScannerException(string message) : Exception(message)
{
    internal static ScannerException TokenMessage(Token token, string message)
    {
        var text = token.TokenType == TokenType.EOF ? 
            $"{message}: at end" : 
            $"{message}: at '{token.Lexeme}'";

        return new ScannerException(text);
    }
}