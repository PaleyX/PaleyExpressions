﻿using PaleyExpressions;

namespace PaleyExpressionsRepl;

public static class Functions
{
    [Function("reverse")]
    public static string? Reverse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var chars = text.ToCharArray();
        Array.Reverse(chars);

        return new string(chars);
    }
}