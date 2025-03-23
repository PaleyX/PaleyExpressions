using PaleyExpressions;

namespace UnitTests
{
    public class ExpressionBuilderTests
    {
        private readonly Dictionary<string, object?> _variables = new()
        {
            { "a", 10 }
        };

        [Theory]
        [InlineData("10", 10d)]
        [InlineData("10+10", 20d)]
        [InlineData("-19", -19d)]
        [InlineData("--23", 23d)]
        [InlineData("\"Hello\"", "Hello")]
        [InlineData("\"Hello \"+\"World\"", "Hello World")]
        public void ArbitraryExpressions(string expression, object? expected)
        {
            var result = Runner.RunExpression(expression, _variables);

            Assert.Equal(expected, result);
        }
    }
}
