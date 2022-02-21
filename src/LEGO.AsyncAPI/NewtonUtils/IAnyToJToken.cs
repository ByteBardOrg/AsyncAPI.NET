namespace LEGO.AsyncAPI.NewtonUtils
{
    using LEGO.AsyncAPI.Models.Any;
    using Newtonsoft.Json.Linq;
    using Array = LEGO.AsyncAPI.Models.Any.Array;
    using Boolean = LEGO.AsyncAPI.Models.Any.Boolean;
    using Double = LEGO.AsyncAPI.Models.Any.Double;
    using Object = LEGO.AsyncAPI.Models.Any.Object;
    using String = LEGO.AsyncAPI.Models.Any.String;

    public static class IAnyToJToken
    {
        public static JToken Map(IAny o)
        {
            return o.AnyType switch
            {
                AnyType.Primitive => (Primitive)o switch
                {
                    String s => ObjectToJToken.Map(s.Value),
                    Long l => ObjectToJToken.Map(l.Value),
                    Double d => ObjectToJToken.Map(d.Value),
                    Boolean b => ObjectToJToken.Map(b.Value),
                    Null n => ObjectToJToken.Map(n.Value),
                    _ => throw new ArgumentOutOfRangeException()
                },
                AnyType.Array => Map(o as Array),
                AnyType.Object => Map(o as Object),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static JToken Map(Array obj)
        {
            JArray tokenArray = new JArray();

            foreach (var item in obj)
            {
                tokenArray.Add(Map(item));
            }

            return tokenArray;
        }

        private static JToken Map(Object obj)
        {
            JObject tokenObject = new JObject();

            foreach (var key in obj.Keys)
            {
                tokenObject.Add(key, Map(obj[key]));
            }

            return tokenObject;
        }
    }
}