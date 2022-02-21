namespace LEGO.AsyncAPI.Resolvers
{
    using LEGO.AsyncAPI.Models.Interfaces;
    using LEGO.AsyncAPI.NewtonUtils;

    public class ExtensionDataGetter
    {
        public IEnumerable<KeyValuePair<object, object>> Getter(object o)
        {
            if (o is not IExtensible extensible)
            {
                return null;
            }

            if (extensible.Extensions == null)
            {
                return null;
            }

            IEnumerable<KeyValuePair<object, object>> obj = new Dictionary<object, object>();
            foreach (var (key, value) in extensible.Extensions)
            {
                obj = obj.Append(new KeyValuePair<object, object>(key, IAnyToJToken.Map(value)));
            }

            return obj;
        }
    }
}