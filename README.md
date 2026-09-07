# PaleyExpressions

[![NuGet Version](https://img.shields.io/nuget/v/PaleyExpressions.svg)](https://www.nuget.org/packages/PaleyExpressions/)

PaleyExpressions is a .NET library distributed as a NuGet package. It contains helpers and utilities for evaluating expressions in .NET applications.

Package
-------

The package is published to NuGet: https://www.nuget.org/packages/PaleyExpressions/

Install
-------

Using the .NET CLI:

	dotnet add package PaleyExpressions

Using Package Manager Console:

	PM> Install-Package PaleyExpressions

Usage
-----

After installing the package, add a reference and import the package namespace in your C# files.
PaleExpressions has 2 ways to evaluate expressions:

- AstRunner - this walks the abstract syntax tree (AST) of the expression and interprets it. 
- ExpressionRunner - this compiles the expression into a Microsoft Expression Tree delegate for faster execution.

```csharp
using PaleyExpressions.Runners;

var vars = new Dictionary<string, object?> { ["x"] = 10 };

// Interpret using the AST-based interpreter
var ast = new AstRunner("x + 2");
var astResult = ast.Interpret(vars);

// Interpret using the compiled Expression runner 
var expr = new ExpressionRunner("x * 2");
var exprResult = expr.Interpret(vars);

Console.WriteLine(astResult); // 12
Console.WriteLine(exprResult); // 20
```
User Defined Functions
----------------------

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

The FunctionAttribute takes a string parameter which is the name of the function as it will be called from expressions. The method must be static and can have any number of parameters. 
A class with user defined functions can be passed to the AstRunner or ExpressionRunner constructor to make the functions available in expressions:

```csharp
var ast = new AstRunner("reverse('hello')", typeof(Functions));
```

Types
-----
- numbers (always C# double)
- strings
- booleans 

Refer to the package documentation and XML docs included in the package for the complete API and examples.

Links
-----

- NuGet package: https://www.nuget.org/packages/PaleyExpressions/
- Source repository: https://github.com/PaleyX/PaleyExpressions

Contributing
------------

Contributions, issues and feature requests are welcome. Please see the source repository for contribution guidelines.

License
-------

See the LICENSE file in the repository for license details.

