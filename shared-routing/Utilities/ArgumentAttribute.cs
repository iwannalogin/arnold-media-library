using System.Collections.Immutable;

namespace arnold.Utilities;

[AttributeUsage(AttributeTargets.Parameter)]
public class ArgumentAttribute(
    string? name = null,
    string? description = null,
    ArgumentType type = ArgumentType.Value,
    bool required = false,
    string[]? aliases = null) : Attribute {

    public string? Name { get; init; } = name;
    public string Description { get; init; } = description ?? string.Empty;
    public ArgumentType Type { get; init; } = type;
    public bool Required { get; init; } = required;
    public ImmutableArray<string> Aliases { get; init; } = [.. aliases ?? []];
}