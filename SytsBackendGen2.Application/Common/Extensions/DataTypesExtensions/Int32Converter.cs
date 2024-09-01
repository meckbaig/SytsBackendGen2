using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Common.Extensions.DataTypesExtensions;

public class Int32Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Dictionary<string, object>);
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        reader.Read();

        while (reader.TokenType == JsonToken.PropertyName)
        {
            string propertyName = reader.Value as string;
            reader.Read();

            object value;

            if (reader.TokenType == JsonToken.Integer)
                value = Convert.ToInt32(reader.Value); // convert to Int32 instead of Int64
            else
                value = serializer.Deserialize(reader); // let the serializer handle all other cases

            result.Add(propertyName, value);
            reader.Read();
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
