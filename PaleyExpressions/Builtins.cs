﻿// ReSharper disable UnusedMember.Global
namespace PaleyExpressions
{
    public static class Builtins
    {
        [Function("abs")]
        public static double Abs(double d) => Math.Abs(d);

        [Function("upper")]
        public static string Upper(string s) => s.ToUpper();

        [Function("lower")]
        public static string Lower(string s) => s.ToLower();

        internal static List<Type> FunctionSources { get; } = [typeof(Builtins)];
    }
}
