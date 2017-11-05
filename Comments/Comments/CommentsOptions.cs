using Microsoft.AspNetCore.Http;
using System;

namespace Comments
{
    public class CommentsOptions
    {
        public int CommentSourceMaxLength { get; set; } = 600;
        public bool LoadCss { get; set; } = true;
        public bool IncludeHashInUrl { get; set; } = false;
        public string BaseUrl { get; set; } = "/comments-middleware";
        public string FilePath { get; set; } = "comments.sqlite";
        public string NoCommentsTemplate { get; set; } = "no comments";
        public string OneCommentTemplate { get; set; } = "1 comment";
        public string MoreThanOneCommentTemplate { get; set; } = "{count} comments";
        public Func<int, string> CommentCountFormater { get; set; }
        public LoadJsDependenciesOptions LoadJsDependencies { get; set; } = LoadJsDependenciesOptions.AutoDetect;
        public Func<HttpContext, bool> IsUserAdminModeratorCheck { get; set; } =
            ctx => ctx.User.Identity.IsAuthenticated && ctx.User.IsInRole("commentsmod");
        // public string DeletedCommentContent { get; set; } = "Comment is deleted by a moderator";
        public bool RequireCommentApproval { get; set; } = false;
        public bool DebugMode { get; set; }
#if DEBUG
            = true;
#else
            = false;
#endif
        public bool DisplayPostCommentDivOnLoad { get; set; } = true;
    }
}
