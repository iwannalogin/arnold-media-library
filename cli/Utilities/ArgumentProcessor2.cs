namespace System;

//public class ArgumentProcessor2 {
//    public static T Execute<T>( T map, IEnumerable<string> args ) {
//        var argumentMap = typeof(T)
//            .GetProperties()
//            .Where( prop => prop.PropertyType.IsAssignableTo( typeof(IArgument) ) )
//            .Select( prop => ( Name: prop.Name, Definition: (IArgument)prop.GetValue(map)! ) );
//        
//        var argumentStack = new Stack<( string Name, IArgument Definition)>();
//        var initialArgument = argumentMap.FirstOrDefault( def => def.Definition.GetType() != typeof(FlagArgument) );
//        if( initialArgument != default ) argumentStack.Push(initialArgument);
//
//        foreach( var arg in args ) {
//            
//        }
//    }
//}

public interface IArgument {
    string Description { get; }
    IEnumerable<string> Aliases { get; }
    bool Required { get; }
    bool HasValue { get; }
}

public interface IArgument<T> : IArgument {
    T Value { get; }

    void SetValue( T value );
}

public class ValueArgument<T> : IArgument<T> {
    public string Description { get; init; }
    public IEnumerable<string> Aliases { get; init; }
    public bool Required { get; init; }
    public bool HasValue { get; private set; }
    
    private T? _value;
    public T Value => _value!;

    public ValueArgument( string description, IEnumerable<string> aliases, bool required = false ) {
        Description = description;
        Aliases = aliases;
        Required = required;
        HasValue = false;
    }

    public ValueArgument( string description, IEnumerable<string> aliases, T defaultValue, bool required = false ) {
        Description = description;
        Aliases = aliases;
        Required = required;
        SetValue(defaultValue);
    }

    public void SetValue( T value ) {
        _value = value;
        HasValue = true;
    }
}

public class FlagArgument( string description, IEnumerable<string> aliases, bool? defaultValue ) : IArgument<bool> {
    public string Description => description;
    public IEnumerable<string> Aliases => aliases;
    public bool Required => false;
    public bool HasValue => true;
    public bool Value { get; private set; } = defaultValue ?? false;

    public void SetValue( bool value ) {
        Value = value;
    }
}

public class ListArgument<T> : IArgument<IEnumerable<T>> {
    public string Description { get; init; }
    public IEnumerable<string> Aliases { get; init; }
    public bool Required { get; init; }
    public bool HasValue { get; private set; }
    private IEnumerable<T>? _value;
    public IEnumerable<T> Value => _value!;

    public ListArgument( string description, IEnumerable<string> aliases, bool required = false ) {
        Description = description;
        Aliases = aliases;
        Required = required;
        HasValue = false;
    }

    public ListArgument( string description, IEnumerable<string> aliases, IEnumerable<T> defaultValue, bool required = false ) {
        Description = description;
        Aliases = aliases;
        Required = required;
        SetValue(defaultValue);
    }

    public void SetValue( IEnumerable<T> value ) {
        _value = value;
        HasValue = true;
    }
}