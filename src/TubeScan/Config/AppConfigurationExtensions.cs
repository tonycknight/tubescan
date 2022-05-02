using Tk.Extensions;

namespace TubeScan.Config
{
    internal static class AppConfigurationExtensions
    {
        public static AppConfiguration Validate(this AppConfiguration config)
        {
            config.InvalidOpArg(c => c == null, "Missing configuration.");

            config.Discord
                    .InvalidOpArg(c => string.IsNullOrWhiteSpace(c?.ClientId), "The Discord Client ID is missing.")
                    .InvalidOpArg(c => string.IsNullOrWhiteSpace(c?.ClientToken), "The Discord Client token is missing.");

            config.Mongo
                    .InvalidOpArg(c => string.IsNullOrWhiteSpace(c?.Connection), "The Mongo connection is missing.")
                    .InvalidOpArg(c => string.IsNullOrWhiteSpace(c?.DatabaseName), "The Mongo DB name is missing.");

            config.Tfl
                    .InvalidOpArg(c => string.IsNullOrWhiteSpace(c?.AppKey), "The TfL App key is missing.");

            return config;
        }
    }
}
