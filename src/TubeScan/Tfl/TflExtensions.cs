namespace TubeScan.Tfl
{
    internal static class TflExtensions
    {
        internal static string CreateUri(this string tflDomain, string route, string appKey)
        {
            var i = route.IndexOf("?");

            var (path, query) = i >= 0
                ? (route.Substring(0, i), route.Substring(i + 1))
                : (route, "");

            var uri = new UriBuilder(tflDomain);
            uri.Path = path;
            uri.Query += query;
            
            if (appKey != null)
            {
                var p = uri.Query.Length > 0 ? "&" : "";
                uri.Query += $"{p}appKey={appKey}";
            }

            return uri.Uri.ToString();
        }
    }
}
