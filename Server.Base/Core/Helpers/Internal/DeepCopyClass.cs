using Newtonsoft.Json;

namespace Server.Base.Core.Helpers.Internal;

public static class DeepCopyClass
{
    public static T DeepCopy<T>(this T other)
    {
        var tmpStr = JsonConvert.SerializeObject(other);
        return JsonConvert.DeserializeObject<T>(tmpStr);
    }
}
