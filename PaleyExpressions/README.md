# PaleyExpressions

A lightweight expression parser and evaluator for .NET (net10.0). PaleyExpressions parses mathematical, logical and string expressions, supports variables, and lets you register static function classes via a small attribute-based API.

Key components
- Parser / AST (internal) for building expression trees
- AstRunner — interprets expressions using an AST visitor
- ExpressionRunner — compiles expressions to System.Linq.Expressions for fast execution
- FunctionAttribute — annotate static methods to expose them as callable functions from expressions

Quick start
1. Clone the repository and build:

   dotnet build

2. Run the REPL example:

   dotnet run --project PaleyExpressionsRepl

Basic usage
Use either AstRunner (simple interpreter) or ExpressionRunner (compiled expressions). Pass an optional Type containing static methods annotated with [Function("name")] to expose custom functions.

Example

```csharp
using PaleyExpressions.Runners;

var vars = new Dictionary<string, object?> { ["x"] = 10 };

// Interpret using the AST-based interpreter
var astResult = new AstRunner("x + 2", typeof(Functions)).Interpret(vars);

// Interpret using the compiled Expression runner (faster for repeated runs)
var exprResult = new ExpressionRunner("x + 2", typeof(Functions)).Interpret(vars);

Console.WriteLine(astResult); // 12
```

Defining functions
Create a static class and annotate methods with FunctionAttribute to make them callable from expressions.

```csharp
using PaleyExpressions;

public static class Functions
{
	[Function("reverse")]
	public static string? Reverse(string? text) => /* ... */ null;

	[Function("format")]
	public static string Format(string fmt, params object?[] args) => string.Format(fmt, args);
}
```

Building and packaging
- Build: dotnet build
- Run tests: dotnet test
- Create NuGet package: dotnet pack (project is configured to include README.md)

Contributing
- Contributions are welcome. Open issues or pull requests on the repository. Keep changes focused and add tests for new behavior.

License
- No license file detected in this repository. Add a LICENSE to declare terms for reuse.

