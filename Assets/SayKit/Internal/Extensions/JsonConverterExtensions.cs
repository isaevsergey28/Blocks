using System.Collections.Generic;
using SayKitInternal;

namespace SayGames.Med.Internal
{
    public static class JsonConverterExtensions
    {
        public static List<object> ArrayListFromJson(this string json)
        {
            return JsonConverter.Deserialize(json) as List<object>;
        }
        public static string GetString(
            this Dictionary<string, object> dic,
            string key,
            string defaultValue = "")
        {
            return dic.ContainsKey(key) ? dic[key].ToString() : defaultValue;
        }
        
        public static string toJson(this Dictionary<string, object> obj)
        {
            return JsonConverter.Serialize(obj);
        }
    }
}