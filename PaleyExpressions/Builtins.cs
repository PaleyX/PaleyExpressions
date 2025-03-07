namespace PaleyExpressions
{
    public static class Builtins
    {
        [Function("abs")]
        public static double Abs(double d) => Math.Abs(d);
    }
}
