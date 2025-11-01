using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using arnold.Utilities;

namespace arnold.Services;

[FormattingService("text")]
public partial class StringFormattingService : IFormattingService {
    public string Separator { get; private set; } = " ";
    public string[] Properties { get; private set; } = [];

    public void Configure( IEnumerable<string> options ) {
        if( !options.Any() ) return;

        var properties = new List<string>();
        foreach( var opt in options ) {
            var separatorMatch = SeparatorRegex().Match(opt);
            if( separatorMatch.Success ) {
                Separator = separatorMatch.Groups["separator"].Value;
            } else {
                properties.Add( opt );
            }
        }
        Properties = [.. properties];
    }

    public string Print( object item ) {
        if( Properties.Length == 0 ) return item?.ToString() ?? "";

        var strBuilder = new StringBuilder();
        var valueProperties = item.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public );

        foreach( var property in Properties ) {
            var propInfo = valueProperties.FirstOrDefault( pi => pi.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase) );
            if( propInfo is null ) {
                strBuilder.Append("[INVALID]" + Separator);
                continue;
            }
            
            var valString = propInfo.GetValue(item)?.ToString() ?? "";

            if( valString.Length == 0 ) {
                strBuilder.Append( string.Empty + Separator );
            } else if( WhitespaceRegex().IsMatch(valString) ) {
                strBuilder.Append( $"\"{valString.Replace("\"", "\\\"")}\"" + Separator );
            } else {
                strBuilder.Append( valString + Separator );
            }
        }

        if( strBuilder.Length > Separator.Length ) strBuilder.Length -= Separator.Length;
        return strBuilder.ToString();
    }

    [GeneratedRegex(@"^sep(arator)?\s*=\s*'?(?<separator>[\s\w]+)'?$")]
    private static partial Regex SeparatorRegex();
    [GeneratedRegex("\\s")]
    private static partial Regex WhitespaceRegex();
}