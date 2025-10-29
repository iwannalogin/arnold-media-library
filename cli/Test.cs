using System.ComponentModel;
using arnold.Utilities;

namespace arnold;

class Test {
    public static CommandRouter ArgumentRouter = new (
        Name: "argument",
        Aliases: [ "arg", "args", "argument", "arguments" ],
        Description: "Test argument parsing",
        Handler: static ( string Name, double[]? Numbers = null, bool Upside = false ) => {
            Console.WriteLine( $"Name: {Name}");
            Console.WriteLine( $"Numbers: {String.Join(", ", Numbers ?? [])}");
            if( Upside ) Console.WriteLine( "UPSIDE" );
        },
        Arguments: [ new (
            Name: "Name",
            Description: "qwerqwer",
            Type: ArgumentType.Value,
            Required: true
        ), new (
            Name: "Numbers",
            Description: "asdfasdf",
            Type: ArgumentType.List
        ), new (
            Name: "Upside",
            Description: "vbnvbn",
            Type: ArgumentType.Flag
        )]
    );
    
    public static CommandRouter EntryRouter = new (
        Name: "test",
        Description: "Assorted tests",
        Aliases: ["test", "tests"],
        SubCommands: [ CommandProcessor.FallbackRouter, ArgumentRouter ],
        Handler: CommandProcessor.FallbackRouter.Handler
    );
}