using static PaleyExpressions.TokenType;

namespace PaleyExpressions;

internal class Parser(List<Token> tokens, Type? functions = null)
{
    private int _current;
    private List<Token> Tokens { get; } = tokens;
    private Type? Functions { get; } = functions;

    internal Expr Parse() => Expression();

    private Expr Expression() => Or();

    private Expr Or()
    {
        var expr = And();

        while (Match(OR))
        {
            var op = Previous();
            var right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(AND))
        {
            var op = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            var op = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (Match(MINUS, PLUS))
        {
            var op = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (Match(SLASH, STAR, MOD))
        {
            var op = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            var op = Previous();
            var right = Unary();
            return new Expr.Unary(op, right);
        }

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            var expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression");
            return new Expr.Grouping(expr);
        }

        throw ScannerException.TokenMessage(Peek(), "Expect expression");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }

        throw ScannerException.TokenMessage(Peek(), message);
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return Peek().TokenType == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            _current++;
        }

        return Previous();
    }

    private bool IsAtEnd() => Peek().TokenType == EOF;

    private Token Peek() => Tokens[_current];

    private Token Previous() => Tokens[_current - 1];

    private Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();

        if (!Check(RIGHT_PAREN))
        {
            do
            {
                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        var paren = Consume(RIGHT_PAREN, "Expect ')' after arguments");

        if (callee is not Expr.Variable variable)
        {
            throw new ScannerException("Empty function name");
        }

        if (Functions != null)
        {
            if (!Builtins.FunctionSources.Contains(Functions))
            {
                Builtins.AddFunctionsClass(Functions);
            }
        }

        var function = Tools.GetFunction(variable.Name.Lexeme, arguments);

        return new Expr.Call(callee, paren, arguments, function);
    }
}
