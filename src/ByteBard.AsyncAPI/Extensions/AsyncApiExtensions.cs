namespace ByteBard.AsyncAPI.Extensions
{
    using System.Text.RegularExpressions;

    public static class AsyncApiExtensions
    {
        private static Regex channelAddressExpressionRegex = new Regex(@"\{[A-Za-z0-9_-]+\}");

        public static bool IsChannelAddressExpression(this string address)
        {
            return address != null && channelAddressExpressionRegex.IsMatch(address);
        }
    }
}
