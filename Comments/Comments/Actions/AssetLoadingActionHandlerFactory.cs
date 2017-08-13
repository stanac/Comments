using Comments.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class AssetLoadingActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;
        private readonly List<string> _knownAssets;
        private readonly string[] _resourceNames;
        private readonly Assembly _assembly;
        private readonly IDictionary<string, string> _assets = new ConcurrentDictionary<string, string>();

        public AssetLoadingActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options, List<string> knownAssets)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataAccessFact = dataAccessFact ?? throw new ArgumentNullException(nameof(dataAccessFact));
            _knownAssets = knownAssets;
            _assembly = GetType().GetTypeInfo().Assembly;
            _resourceNames = _assembly.GetManifestResourceNames();
        }

        public ActionHandler GetActionHandler()
        {
            return new AssetActionHandler(_options.BaseUrl, _knownAssets)
            {
                RequestMethod = "get",
                RequestUrl = (_options.BaseUrl + "/count").NormalizePath(),
                HandleRequest = HandleRequest
            };
        }

        public Task HandleRequest(HttpContext ctx)
        {
            try
            {
                string assetName = ctx.Request.Path.ToString().Substring(_options.BaseUrl.Length).ToLower();
                string asset = GetResource(assetName);
                string mime = "text/plain";
                if (assetName.EndsWith(".js", StringComparison.Ordinal))
                {
                    mime = "text/javascript";
                }
                else if (assetName.EndsWith(".css", StringComparison.Ordinal))
                {
                    mime = "text/css";
                }
                return ctx.Response.WriteResponse(asset, mime, 200);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load resource: '{ctx.Request.Path}'", ex);
            }
        }

        private string GetResource(string assetName)
        {
            assetName = assetName.TrimStart('/');
            assetName = "." + assetName.ToLower();
            var resourceName = _resourceNames.Single(x => x.EndsWith(assetName, StringComparison.Ordinal));
            if (!_assets.ContainsKey(resourceName))
            {
                using (Stream s = _assembly.GetManifestResourceStream(resourceName))
                using (TextReader reader = new StreamReader(s))
                {
                    string resource = reader.ReadToEnd();
                    if (assetName == ".loader.js") resource = SetLoaderSettings(resource);
                    else if (assetName == ".comments.js") resource = SetCommentsSettings(resource);
                    _assets[resourceName] = resource;
                    return resource;
                }
            }
            return _assets[resourceName];
        }

        private string SetCommentsSettings(string resource)
        {
            string oldValue = "var baseUrl = '/comments-middleware'; // replace this";
            string newValue = $"var baseUrl = '{_options.BaseUrl}';";
            return resource.Replace(oldValue, newValue);
        }

        private string SetLoaderSettings(string loader)
        {
            Func<LoadJsDependenciesOptions, string> formatJsDependencyOptions =
                option => option == LoadJsDependenciesOptions.AutoDetect ? "auto" : option.ToString().ToLower();

            string loadCss = _options.LoadCss.ToString().ToLower();
            string loadMinified = (!_options.DebugMode).ToString().ToLower();
            string oldSettings = "var options = { loadJs: 'auto', middlewareRoot: '/comments-middleware', loadMinified: true, loadCss: true }; // replace this";
            string newSettings = $"var options = {{ loadJs: '{formatJsDependencyOptions(_options.LoadJsDependencies)}', middlewareRoot: "
                + $"'{_options.BaseUrl}', loadMinified: {loadMinified}, loadCss: {loadCss} }};";
            loader = loader.Replace(oldSettings, newSettings);
            return loader;
        }
    }
}
