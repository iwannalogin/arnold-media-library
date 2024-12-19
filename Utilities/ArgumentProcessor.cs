namespace System;

public enum ArgumentType { Flag, List, Value }
public readonly record struct ArgumentDefinition( string Name, string Description, ArgumentType Type, params string[] Aliases );

public class ArgumentProcessor( IEnumerable<ArgumentDefinition> ArgumentMap ) {
    public static readonly Dictionary<string, IEnumerable<string>?> HelpResult = new Dictionary<string, IEnumerable<string>?>();

    public Dictionary<string,IEnumerable<string>?> Execute( string[] args ) {
        var argumentStack = new Stack<ArgumentDefinition>();
        {
            var initialArgument = ArgumentMap.FirstOrDefault( def => def.Type == ArgumentType.List || def.Type == ArgumentType.Value );
            if( initialArgument != default ) argumentStack.Push( initialArgument );
        }
        var output = new Dictionary<string, IEnumerable<string>?>();
        foreach( var def in argumentStack ) {
            if( def.Type == ArgumentType.List ) output.Add( def.Name, new List<string>() );
            else if( def.Type == ArgumentType.Value ) output.Add( def.Name, default );
        }

        string[] helpAliases = ["help", "-h", "--h", "-help", "--help", "-?", "--?"];

        foreach( var arg in args ) {
            var testArg = arg.ToLower();

            if( helpAliases.Contains( testArg ) ) {
                return HelpResult;
            }

            foreach( var def in ArgumentMap ) {
                if( def.Aliases.Any( alias => alias.ToLower() == testArg ) ) {
                    if( def.Type == ArgumentType.Flag ) {
                        output.Add( def.Name, ["true"] );
                    } else {
                        argumentStack.Push( def );
                    }
                }
                continue;
            }

            if( argumentStack.Any() == false ) continue;

            var activeArgument = argumentStack.Peek();
            if( activeArgument.Type == ArgumentType.Value ) {
                output[activeArgument.Name] = [arg];
                argumentStack.Pop();
            } else if( activeArgument.Type == ArgumentType.List ) {
                ((List<string>)output[activeArgument.Name]!).Add( arg );
            }
        }

        return output;
    }

    public static Dictionary<string,IEnumerable<string>?> Process( string[] args, IEnumerable<ArgumentDefinition> argumentMap ) {
        var processor = new ArgumentProcessor( argumentMap );
        return processor.Execute(args);
    }
}