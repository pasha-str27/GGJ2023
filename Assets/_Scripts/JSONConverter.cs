using Newtonsoft.Json;

static public class JSONConverter
{
    public static string ToJson(object obj) => JsonConvert.SerializeObject(obj);

    public static T FromJson<T>(string str) => JsonConvert.DeserializeObject<T>(str);
}
