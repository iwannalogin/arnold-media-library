using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace arnold.Utilities;

public enum ArgumentType { Flag, List, Value };

public enum CliRepresentation { Argument, Option };

public class ArgumentDefinition {
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ArgumentType Type { get; init; } = ArgumentType.Value;
    public bool Required { get; init; } = false;
    public ImmutableArray<string> Aliases { get; init; } = [];
    public CliRepresentation Representation { get; set; } = CliRepresentation.Option; 

    public ArgumentDefinition( ParameterInfo parameterInfo ) {
        var argAttribute = parameterInfo.GetCustomAttribute<ArgumentAttribute>();

        Name = argAttribute?.Name ?? parameterInfo.Name!;
        Description = argAttribute?.Description
            ?? parameterInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
            ?? string.Empty;
        Type = argAttribute?.Type ?? InferType( parameterInfo.ParameterType );

        Required = IsRequired( parameterInfo, argAttribute );
        
        var aliases = new List<string> { Name };
        if( argAttribute is not null ) aliases.AddRange( argAttribute.Aliases );
        Aliases = [.. aliases];
    }

    public static bool IsRequired( ParameterInfo param )
        => IsRequired( param, param.GetCustomAttribute<ArgumentAttribute>() );

    public static bool IsRequired( ParameterInfo param, ArgumentAttribute? argAttr )
        => (argAttr?.Required ?? false )
            || param.GetCustomAttribute<RequiredAttribute>() is not null
            || !param.HasDefaultValue;

    private static ArgumentType InferType( Type type ) {
        if( type == typeof(bool) ) return ArgumentType.Flag;
        else if( type == typeof(string) ) return ArgumentType.Value;
        else if( type.IsEnum ) return type.GetCustomAttribute<FlagsAttribute>() is null ? ArgumentType.Value : ArgumentType.List;
        else if( type.IsArray ) return ArgumentType.List;
        else if( type.IsAssignableTo( typeof(IEnumerable) ) ) return ArgumentType.List;
        throw new InvalidOperationException( $"Failed to infer ArgumentType from typeof({type})" );
    }
}