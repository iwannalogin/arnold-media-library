using arnold.Utilities;

namespace System;

public enum ArgumentType { Flag, List, Value }
public readonly record struct ArgumentDefinition(
    string Name,
    string Description,
    ArgumentType Type,
    bool Required = false,
    IEnumerable<string>? Aliases = null ) {
    public IEnumerable<string> TestAliases {
        get => [ $"--{ToKebabCase(Name)}", .. Aliases ?? [] ];
    }

    public static string ToKebabCase( string value )
        => value.ToLower().Replace(" ", "-");
};

public class ArgumentProcessor( IEnumerable<ArgumentDefinition> ArgumentMap ) {
    public static readonly Dictionary<string, IEnumerable<string>?> HelpResult = [];

    public Dictionary<string,IEnumerable<string>?> Execute( string[] args ) {
        var argumentStack = new Stack<ArgumentDefinition>();
        {
            var initialArgument = ArgumentMap.FirstOrDefault( def => def.Type == ArgumentType.List || def.Type == ArgumentType.Value );
            if( initialArgument != default ) argumentStack.Push( initialArgument );
        }
        var output = new Dictionary<string, IEnumerable<string>?>();
        foreach( var def in argumentStack ) {
            if( def.Type == ArgumentType.List ) output.Add( def.Name, [] );
            else if( def.Type == ArgumentType.Value ) output.Add( def.Name, default );
        }

        string[] helpAliases = ["help", "-h", "--h", "-help", "--help", "-?", "--?"];

        foreach( var arg in args ) {
            var testArg = arg.ToLower();

            if( helpAliases.Contains( testArg ) ) {
                return HelpResult;
            }

            var isKey = false;
            foreach( var def in ArgumentMap ) {
                if( def.TestAliases.Any( alias => alias.Equals(testArg, StringComparison.CurrentCultureIgnoreCase)) ) {
                    if( def.Type == ArgumentType.Flag ) {
                        output.Add( def.Name, ["true"] );
                    } else {
                        argumentStack.Push( def );
                        if( output.ContainsKey( def.Name ) == false ) {
                            if( def.Type == ArgumentType.List ) output.Add(def.Name, new List<string>() );
                            else if( def.Type == ArgumentType.Value ) output.Add( def.Name, default );
                        }
                    }
                    isKey = true;
                }
            }
            if( isKey ) continue;

            if( argumentStack.Count == 0 ) continue;

            var activeArgument = argumentStack.Peek();
            if( activeArgument.Type == ArgumentType.Value ) {
                output[activeArgument.Name] = [arg];
                argumentStack.Pop();
            } else if( activeArgument.Type == ArgumentType.List ) {
                ((List<string>)output[activeArgument.Name]!).Add( arg );
            }
        }

        return output!;
    }

    public static Dictionary<string,IEnumerable<string>?> Process( string[] args, IEnumerable<ArgumentDefinition> argumentMap ) {
        var processor = new ArgumentProcessor( argumentMap );
        var arguments = processor.Execute(args);

        var missingArguments = argumentMap
            .Where( arg => arg.Required && !arguments.ContainsKey(arg.Name) )
            .Select( arg => arg.Name );

        if( missingArguments.Any() ) throw new MissingArgumentsException(missingArguments);
        return arguments!;
    }

    //public static T Process<T>( string[] args, IEnumerable<ArgumentDefinition> argumentMap ) {
    //    var asDictionary = new ArgumentProcessor( argumentMap ).Execute( args );
    //    var output = default(T);
    //    var outputType = typeof(T);
//
    //    foreach( var kvp in asDictionary ) {
    //        var argumentDef = argumentMap.First( def => def.Name.ToLower() == kvp.Key.ToLower() );
//
    //        object? outputValue = argumentDef.Type switch {
    //            ArgumentType.Flag => true,
    //            ArgumentType.List => kvp.Value,
    //            ArgumentType.Value => kvp.Value?.FirstOrDefault(),
    //            _ => null
    //        };
//
    //        var outputProp = outputType
    //                            .GetProperties()
    //                            .FirstOrDefault( prop => prop.Name.ToLower() == kvp.Key.ToLower() );
//
    //        var outputField = (outputProp is not null) ? null
    //                            : outputType
    //                                .GetFields()
    //                                .FirstOrDefault( field => field.Name.ToLower() == kvp.Key.ToLower() );
//
    //        Action<object>? setProperty = null;
    //        Type? propertyType = null;
    //        if( outputProp is not null ) {
    //            setProperty = val => outputProp.SetValue( output, val );
    //            propertyType = outputProp.PropertyType;
    //        } else if( outputField is not null ) {
    //            setProperty = val => outputField.SetValue(output, val );
    //            propertyType = outputField.FieldType;
    //        }
//
    //        if( setProperty is null || propertyType is null ) {
    //            continue;
    //        }
//
    //        setProperty( propertyType switch {
    //            System.String => outputValue is string ? outputValue : outputValue?.ToString(),
    //        } );
    //    }
    //}

    //protected static object? ConvertToType( object? value, Type targetType ) {
    //    if( value is null ) return Activator.CreateInstance(targetType);
    //    else if( targetType == typeof(string) ) return value.ToString();
    //    else if( targetType == typeof( IEnumerable<string> ) ) {
    //        if( value.GetType() == typeof(List<string>) ) return value;
    //        else if( value.GetType() == typeof(string) ) return new string[] {(string)value};
    //    } 
    //}
}