using arnold.Utilities;

namespace arnold.Services;

[FormattingService("json")]
public class JsonFormattingService : IFormattingService {
    public Newtonsoft.Json.Formatting Formatting { get; private set; } = Newtonsoft.Json.Formatting.None;
    public bool NoComma { get; private set; } = false;

    public void Configure( IEnumerable<string> options ) {
        foreach( var option in options ) {
            if( option.Equals( "pretty", StringComparison.InvariantCultureIgnoreCase ) ) {
                Formatting = Newtonsoft.Json.Formatting.Indented;
            } else if( option.Equals( "no-comma", StringComparison.InvariantCultureIgnoreCase ) ) {
                NoComma = true;
            }
        }
    }

    public string Print( object item )
        => Newtonsoft.Json.JsonConvert.SerializeObject( item ) + (NoComma ? "" : ", ");
}