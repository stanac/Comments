using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Comments.Actions
{
    class AssetActionHandler: ActionHandler
    {
        public IReadOnlyList<string> KnownAssets { get; private set; }
        public string BaseUrl { get; private set; }

        public AssetActionHandler(string baseUrl, IEnumerable<string> knownAssets)
        {
            BaseUrl = baseUrl;
            KnownAssets = knownAssets.ToList();
        }

        public override bool ShouldHandleRequest(HttpRequest request)
        {
            if (request.Method != "GET") return false;

            return KnownAssets.Any(x => request.Path.ToString().ToLower() == (BaseUrl + x).ToLower());
        }
    }
}
