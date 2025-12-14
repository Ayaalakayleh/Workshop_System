
using Microsoft.Extensions.Localization;

namespace Workshop.Resources
{
    public static class ResourceProvider
    {
        private static IStringLocalizer? _commonLocalizer;
        private static IStringLocalizer? _messagesLocalizer;

        public static void Initialize(IStringLocalizerFactory factory)
        {
            var type = typeof(ResourceProvider);
            _commonLocalizer = factory.Create("Common.Common", type.Assembly.FullName!);
            _messagesLocalizer = factory.Create("Messages.Messages", type.Assembly.FullName!);
        }

        public static string GetCommonResource(string key)
            => _commonLocalizer?[key].Value ?? key;

        public static string GetMessage(string key)
            => _messagesLocalizer?[key].Value ?? key;
    }
}
