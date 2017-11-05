using Comments;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtenstions
    {
        public static IApplicationBuilder UseComments(this IApplicationBuilder appBuilder)
            => UseComments(appBuilder, o => { });

        public static IApplicationBuilder UseComments(this IApplicationBuilder appBuilder, Action<CommentsOptions> setOptions)
        {
            var options = new CommentsOptions();
            setOptions(options);
            appBuilder.UseMiddleware<CommentsMiddlware>(options);
            return appBuilder;
        }
    }
}
