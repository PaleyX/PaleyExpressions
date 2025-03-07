namespace PaleyExpressions;

public class FunctionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}