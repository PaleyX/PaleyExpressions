using static PaleyExpressions.TokenType;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var x = new PaleyExpressions.Scanner("1+1+gort");

            var tokens = x.ScanTokens();

            Assert.Equal(5 + 1, tokens.Count);

            Assert.Equal(NUMBER, tokens[0].TokenType);
        }
    }
}
