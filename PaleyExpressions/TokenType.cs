namespace PaleyExpressions;

internal enum TokenType
{
    // Single-character tokens.
    LEFT_PAREN, RIGHT_PAREN, 
    COMMA, MINUS, PLUS, SLASH, STAR, MOD,
    
    // Bitwise
    BITWISE_AND, BITWISE_OR, BITWISE_XOR, BITWISE_NOT,

    // One or two character tokens.
    BANG, BANG_EQUAL,
    EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL,
    LESS, LESS_EQUAL,
    LEFT_SHIFT, RIGHT_SHIFT,

    // Literals.
    IDENTIFIER, STRING, NUMBER,

    // Keywords.
    AND, FALSE, NIL, OR, TRUE, 

    EOF
}