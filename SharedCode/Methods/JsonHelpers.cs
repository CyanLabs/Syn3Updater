using Newtonsoft.Json;
using System.IO;

namespace Cyanlabs.Updater.Common
{
    public static class JsonHelpers
    {
        public static T Deserialize<T>(Stream s)
        {
            using StreamReader reader = new StreamReader(s);
            using JsonTextReader jsonReader = new JsonTextReader(reader);
            JsonSerializer ser = new JsonSerializer();
            return ser.Deserialize<T>(jsonReader);
        }

        public static void Serialize(object value, Stream s)
        {
            using StreamWriter writer = new StreamWriter(s);
            using JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            JsonSerializer ser = new JsonSerializer();
            ser.Serialize(jsonWriter, value);
            jsonWriter.Flush();
        }
    }
}
