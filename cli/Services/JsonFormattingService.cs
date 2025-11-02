using arnold.Utilities;
using Newtonsoft.Json;

namespace arnold.Services;

[FormattingService("json")]
public class JsonFormattingService : IFormattingService {
    public Formatting Formatting { get; private set; } = Formatting.None;
    public JsonSerializerSettings Settings { get; private set; }

    public JsonFormattingService() {
        Settings = new JsonSerializerSettings();
        Settings.Converters.Add( new ComplexListToCountConverter() );
    }

    public bool NoComma { get; private set; } = false;

    public void Configure( IEnumerable<string> options ) {
        foreach( var option in options ) {
            if( option.Equals( "pretty", StringComparison.InvariantCultureIgnoreCase ) ) {
                Formatting = Formatting.Indented;
            } else if( option.Equals( "no-comma", StringComparison.InvariantCultureIgnoreCase ) ) {
                NoComma = true;
            }
        }
    }

    public string Print( object item )
        => JsonConvert.SerializeObject( item, Settings ) + (NoComma ? "" : ", ");
}