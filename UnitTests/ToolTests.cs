using FluentAssertions;
using PaleyExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class ToolTests
    {
        public ToolTests()
        {
            Builtins.FunctionSources.Remove(typeof(ToolsTestsFunctions));
            Builtins.AddFunctionsClass(typeof(ToolsTestsFunctions));
        }

        [Fact]
        public void TooFewArgumentsThrowsCorrectly()
        {
            var ex = Assert.Throws<ScannerException>(() => Tools.GetFunction("f1", []));

            Assert.Equal("Function 'f1': argument count mismatch", ex.Message);
        }

        [Fact]
        public void OneArgWithNoParamsProvidedIsOk()
        {
            var args = new List<Expr>
            {
                   new Expr.Literal("First")
            };

            var func = Tools.GetFunction("f1", args);

            // Really, the test is that an exception is not thrown
            func.Name.Should().Be(nameof(ToolsTestsFunctions.F1));

            var expr = new Expr.Call(null, null, args, func);

            var interpreter = new Interpreter();
            var result = interpreter.VisitCallExpr(expr);

            result.Should().Be("First");
        }

        [Fact]
        public void NoArgumentsPassedToPurelyParamsIsOk()
        {
            var func = Tools.GetFunction("f2", []);

            // Really, the test is that an exception is not thrown
            func.Name.Should().Be(nameof(ToolsTestsFunctions.F2));

            var expr = new Expr.Call(null, null, [], func);

            var interpreter = new Interpreter();
            var result = interpreter.VisitCallExpr(expr);

            result.Should().Be("");
        }
    }

    public class ToolsTestsFunctions
    {
        [Function("f1")]
        public static string F1(string text, params Func<object>[] args)
        {
            text += " " + string.Join(" ", args.Select(a => a()));

            return text.Trim();
        }

        [Function("f2")]
        public static string F2(params Func<object>[] args)
        {
            return string.Join(" ", args.Select(a => a()));
        }
    }
}
