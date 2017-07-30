using Comments;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtenstions
    {
        public static IApplicationBuilder UseComments(this IApplicationBuilder appBuilder)
            => UseComments(appBuilder, new CommentsOptions());

        public static IApplicationBuilder UseComments(this IApplicationBuilder appBuilder, CommentsOptions options)
        {
            appBuilder.UseMiddleware<CommentsMiddlware>(options);
            return appBuilder;
        }
    }
}
