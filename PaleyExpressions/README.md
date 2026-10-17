# PaleyExpressions

[![NuGet Version](https://img.shields.io/nuget/v/PaleyExpressions.svg)](https://www.nuget.org/packages/PaleyExpressions/)

PaleyExpressions is a .NET library distributed as a NuGet package. It contains helpers and utilities for evaluating runtime supplied expressions in .NET applications. It supports user defined functions and passing in variables via a dictionary or a user defined class.

## Table of contents
- [Package](#package)
- [Usage](#usage)
- [Variables](#variables)
- [User Defined Functions](#user-defined-functions)
- [Types](#types)
- [Operators](#operators)
- [Built-in Functions](#built-in-functions)
- [Examples](#examples)
- [REPL Example](#simple-repl-example)

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

PaleyExpressions has 4 ways to evaluate expressions - each of the evaluation methods uses
a common scanner and parser to build an abstract syntax tree (AST). That AST is then used by the
various runners to evaluate the expression. The runners are:

|Runner|Description|Try it|
|------|-----------|------|
|AstRunner|Walks the AST and interprets it, variables supplied via dictionary|[Try](https://dotnetfiddle.net/J9gxYM)|
|AstRunner&lt;T&gt;|Generic version of AstRunner that reads variable values from an object of type T|[Try](https://dotnetfiddle.net/PYZTSd)|
|ExpressionRunner|Compiles the AST into a Microsoft Expression Tree delegate for faster execution, variables supplied via dictionary|[Try](https://dotnetfiddle.net/Yb2H5d)|
|ExpressionRunner&lt;T&gt;|Generic version of ExpressionRunner that reads variable values from an object of type T|[Try](https://dotnetfiddle.net/z0E73O)|

The expression is passed as a string to the constructor of the runner. 
The first time the Interpret method is called, the expression is parsed and compiled into an AST or Expression Tree.
Subsequent calls to Interpret will use the cached AST or Expression Tree for faster execution.

Variables
---------

When using the dictionary-based runners, variables are passed to the Interpret method as a Dictionary<string, object?>. 
The variable names are case sensitive and must begin with an alphabetic character or an underscore. 

When using the generic runners, the variable values are read from an object of type T passed to the Interpret method. The property or field names of the object are used as variable names.
Private, internal and public fields and properties are supported. 

In all cases the types supported for variables are: double, string and bool. The value of a variable can be null, in which case it will be treated as nil in the expression.

```csharp
using PaleyExpressions.Runners;

// Note: numeric variables are always C# double, so 10 is 10d
var vars = new Dictionary<string, object?> { ["x"] = 10d };

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
|Type|Literal examples|Explanation|
|----|----------------|-----------|
|Number|10, 3456.765, 0xFF|.Net double|
|String|"", "Hello"||
|Boolean|true, false||
|Null|nil||

Operators
---------
|Operator|Explanation|Example|Result|
|:---|:---|:---|:---|
|+|	numeric addition|	5 + 3|	8|
|+|	string concatenation|	"Hello, " + "World!"|	"Hello, World!"|
|-|	numeric subtraction|	5 - 3|	2|
|-|	unary minus|	-5|	-5|
|*|	numeric multiplication|	5 * 3|	15|
|/|	numeric division|	5 / 3|	1.67|
|%|	modulus (remainder)|	5 % 3|	2|
|( )|	grouping operator|	(5 + 3) * 2|	16|
|<<|left shift (number)|	5 << 2|	20|
|>>|right shift (number)|	5 >> 2|	1|
|<<|left shift (string)|	"Hello" << 1|	"ello"|
|>>|right shift (string)|	"Hello" >> 1|	"Hell"|
|==|equality comparison|	5 == 3|	false|
|!=|inequality comparison|	5 != 3|	true|
|<|less than comparison|5 < 3|	false|
|>|greater than comparison|	5 > 3|	true|
|<=|less than or equal comparison|5 \<= 3|	false|
|>=|greater than or equal comparison|5 >= 3|	true|
|and|logical AND|true and false|	false|
|or|logical OR|	true or false|	true|
|!|	logical NOT|!true|	false|
|&|	bitwise AND|5 & 3|1|
|\||bitwise OR|	5 \| 3|	7|
|~|bitwise NOT| ~5 & 255| 250|

Built-in Functions
------------------
- `abs(numeric expression)` 

   returns the absolute value of a number
- `upper(string expression)`

   returns the uppercase version of a string
- `lower(string expression)`

   returns the lowercase version of a string
- `iif(predicate expression, expression 1, expression 2)`

   returns expression 1 if predicate is true, otherwise returns expression 2.
   This function is special cased in the ExpressionRunner to build an Expression.Condition 
   instead of a MethodCallExpression for better performance.
- `cond(predicate expression, expression ...)`

   takes 1..n pairs of predicate/value arguments and returns the value corresponding to the first predicate that evaluates to true. If no predicates are true, returns null.

Examples
--------
Note: whitespace between tokens within an expression is ignored

[Try examples using DotNet Fiddle](https://dotnetfiddle.net/pVLaSz)

- `sqr(10*(1+2.9))` 
- `lower(s + " " + "World!")`
- `iif(x > 10, upper("x is greater than 10"), upper("x is less than or equal to 10"))`
- `1 > 2 and 3 < 4`
- `true and !false`

Simple REPL example
-------------------
This shows how to write a simple REPL for the expression evaluators. 
It adds some variables and a custom function then runs both evaluators using them.

[Try REPL using DotNet Fiddle](https://dotnetfiddle.net/ZeGHuZ)

```csharp
using System;
using System.Collections.Generic;
using PaleyExpressions;
using PaleyExpressions.Runners;

var variables = new Dictionary<string, object?>
{
    {"n", 10.567d},
    {"s", "hello"}
};

while (true)
{
    try
    {
        var expression = Console.ReadLine();

        var ast = new AstRunner(expression, typeof(Functions))
            .Interpret(variables);
        var exp = new ExpressionRunner(expression, typeof(Functions))
            .Interpret(variables);

        Console.WriteLine($"AST: {ast}");
        Console.WriteLine($"EXP: {exp}");
    }
    catch (Exception e)
    {
        Console.WriteLine("Error: " + e.Message);
    }
}

public static class Functions
{
    [Function("sqr")]
    public static double Square(double d) => d * d;
}
```

Links
-----

- NuGet package: https://www.nuget.org/packages/PaleyExpressions/
- Source repository: https://github.com/PaleyX/PaleyExpressions
- Crafting Interpreters: https://craftinginterpreters.com/

  Some of the code in this library is based on the book "Crafting Interpreters" by Bob Nystrom. The book is an excellent resource for learning how to build interpreters and compilers.

Contributing
------------

Contributions, issues and feature requests are welcome. Please see the source repository for contribution guidelines.

License
-------

See the LICENSE file in the repository for license details.

