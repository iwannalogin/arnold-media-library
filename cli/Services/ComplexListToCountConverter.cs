using Newtonsoft.Json;

namespace arnold.Services;

public class ComplexListToCountConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
        return
            objectType.IsAssignableTo( typeof(System.Collections.ICollection) )
            && objectType.IsGenericType
            && objectType.GenericTypeArguments.Count() == 1
            && !objectType.GenericTypeArguments[0].IsPrimitive;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        if( value is System.Collections.ICollection collection ) {
            writer.WriteValue( collection.Count );
        } else {
            writer.WriteValue( "" );
        }
    }
}