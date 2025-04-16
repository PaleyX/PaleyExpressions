using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaleyExpressions
{
    public class AstRunner(string source, Type? functions)
    {
        private readonly string _source = source;
        private Interpreter _interpreter;
        private Expr? _expr;

        public void GetAst()
        {
            _expr = Parse(_source, functions);
        }

        public object? Interpret(Dictionary<string, object?>? variables)
        {
            _interpreter ??= new Interpreter();

            return _interpreter.Interpret(_expr, variables);
        }

        private static Expr Parse(string source, Type? functions = null)
        {
            var tokens = new Scanner(source).ScanTokens();
            var parser = new Parser(tokens, functions);

            return parser.Parse();
        }
    }
}
