namespace ByteBard.AsyncAPI.Extensions
{
    using System.Text.RegularExpressions;

    public static class AsyncApiExtensions
    {
        private static Regex channelAddressExpressionRegex = new Regex("{[a-zA-Z1-9_-]*}");

        public static bool IsChannelAddressExpression(this string address)
        {
            return address != null && channelAddressExpressionRegex.IsMatch(address);
        }
    }
}
