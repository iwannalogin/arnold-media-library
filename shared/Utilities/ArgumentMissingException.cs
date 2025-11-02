namespace arnold.Utilities;

public class ArgumentMissingException( params IEnumerable<string> arguments ) : Exception {
    public string[] Arguments => [.. arguments];

    public static void Test( (string name, string? value)[] arguments ) {
        var argList = new List<string>();
        foreach( var arg in arguments ) {
            if( string.IsNullOrWhiteSpace(arg.value) ) {
                argList.Add(arg.name);
            }
        }
        if( argList.Any() ) throw new ArgumentMissingException(argList);
    }

    public static void Test( string name, string? value ) {
        if( string.IsNullOrWhiteSpace(value) ) {
            throw new ArgumentMissingException(name);
        }
    }
}