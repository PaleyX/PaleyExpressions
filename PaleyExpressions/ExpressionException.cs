namespace PaleyExpressions;

public class ExpressionException(string message) : Exception(message)
{
    internal static ExpressionException TokenMessage(Token token, string message)
    {
        var text = token.TokenType == TokenType.EOF ? 
            $"{message}: at end" : 
            $"{message}: at '{token.Lexeme}'";

        return new ExpressionException(text);
    }
}