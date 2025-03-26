using PaleyExpressions;

namespace UnitTests
{
    public class ArbitraryExpressionsTests
    {
        private readonly Dictionary<string, object?> _variables = new()
        {
            { "a", 10d },
            { "b", 20d },
            { "c", 3.5d }
        };

        [Theory]
        [InlineData("10", 10d)]
        [InlineData("10+10", 20d)]
        [InlineData("-19", -19d)]
        [InlineData("--23", 23d)]
        [InlineData("\"Hello\"", "Hello")]
        [InlineData("\"Hello \"+\"World\"", "Hello World")]
        [InlineData("10/(9-1)", 1.25d)]
        [InlineData("a+b+c", 33.5d)]
        [InlineData("(b/a)+c", 5.5d)]
        public void ArbitraryExpressions(string expression, object? expected)
        {
            var expressionsResult = Runner.RunExpression(expression, _variables);
            var astResult = Runner.RunAst(expression, _variables);

            Assert.Equal(astResult, expressionsResult);
            Assert.Equal(expected, astResult);
            Assert.Equal(expected, expressionsResult);
        }
    }
}
