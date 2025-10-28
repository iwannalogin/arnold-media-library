namespace arnold.Utilities;

public class MissingArgumentsException : Exception {
    public IEnumerable<string> Arguments { get; init; }
    public MissingArgumentsException( params IEnumerable<string> arguments )
        :base( string.Join( Environment.NewLine, arguments.Select( arg => $"Missing required argument '{arg}'" ) ) ) {
        Arguments = arguments;
    }
}