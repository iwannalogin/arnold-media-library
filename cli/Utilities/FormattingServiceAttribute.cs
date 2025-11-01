namespace arnold.Utilities;

[AttributeUsage(AttributeTargets.Class)]
public class FormattingServiceAttribute( string key ) : Attribute {
    public string Key => key;
}