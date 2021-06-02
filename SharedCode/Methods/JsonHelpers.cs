using System.IO;
using Newtonsoft.Json;

namespace Cyanlabs.Updater.Common
{
    public static class JsonHelpers
    {
        public static T Deserialize<T>(Stream s)
        {
            using StreamReader reader = new(s);
            using JsonTextReader jsonReader = new(reader);
            JsonSerializer ser = new();
            return ser.Deserialize<T>(jsonReader);
        }

        public static void Serialize(object value, Stream s)
        {
            using StreamWriter writer = new(s);
            using JsonTextWriter jsonWriter = new(writer);
            JsonSerializer ser = new();
            ser.Serialize(jsonWriter, value);
            jsonWriter.Flush();
        }
    }
}