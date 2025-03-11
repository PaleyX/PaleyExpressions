namespace PaleyExpressions;

internal class Token(TokenType tokenType, string lexeme, object? literal)
{
    internal TokenType TokenType { get; } = tokenType;
    internal string Lexeme { get; } = lexeme;
    internal object? Literal { get; } = literal;

    public override string ToString()
    {
        return $"{TokenType} {Lexeme} {Literal}";
    }
}