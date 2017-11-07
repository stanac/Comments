using Markdig;
using Microsoft.AspNetCore.Http;
using System;

namespace Comments
{
    /// <summary>
    /// Options for <see cref="CommentsMiddlware"/>
    /// </summary>
    public class CommentsOptions
    {
        /// <summary>
        /// Gets or sets the maximum length of the comment source.
        /// </summary>
        /// <value>
        /// The maximum length of the comment source.
        /// </value>
        public int CommentSourceMaxLength { get; set; } = 600;

        /// <summary>
        /// Gets or sets a value indicating whether to load CSS. Default is true.
        /// </summary>
        /// <value>
        ///   If <c>true</c> loads CSS.
        /// </value>
        public bool LoadCss { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether hash part of URL should be included when defining URL for comment posting and loading.
        /// </summary>
        /// <value>
        ///   If <c>true</c> hash part of URL will be used to identify page when loading and posting comments.
        /// </value>
        public bool IncludeHashInUrl { get; set; } = false;
        /// <summary>
        /// Gets or sets the base URL of the middleware, default is "/comments-middleware".
        /// </summary>
        /// <value>
        /// The base URL of the middleware.
        /// </value>
        public string BaseUrl { get; set; } = "/comments-middleware";
        /// <summary>
        /// Gets or sets the file path of Sqlite DB which stores comments. Default value is "comments.sqlite".
        /// </summary>
        /// <value>
        /// The file path of Sqlite DB which stores comments.
        /// </value>
        public string SqliteDbFilePath { get; set; } = "comments.sqlite";
        /// <summary>
        /// Gets or sets value which will be used to display number of comments when number is zero. Default is "no comments".
        /// </summary>
        /// <value>
        /// The no comments template.
        /// </value>
        public string NoCommentsTemplate { get; set; } = "no comments";
        /// <summary>
        /// Gets or sets the one comment template, which is used to display number of comments, when there is only one comment. Default is "1 comment".
        /// </summary>
        /// <value>
        /// The one comment template.
        /// </value>
        public string OneCommentTemplate { get; set; } = "1 comment";
        /// <summary>
        /// Gets or sets the more than one comment template, which is used to display number of comments when there is more than one comment. Default is "{count} comments".
        /// </summary>
        /// <value>
        /// The more than one comment template. Template should contain "{count}" which will be replaced with comment count.
        /// </value>
        public string MoreThanOneCommentTemplate { get; set; } = "{count} comments";
        /// <summary>
        /// Gets or sets the comment count formatter. If set it will be used to display number of comments.
        /// </summary>
        /// <value>
        /// The comment count formatter.
        /// </value>
        public Func<int, string> CommentCountFormatter { get; set; }
        /// <summary>
        /// Gets or sets whether to the load js dependencies. Default is <see cref="LoadJsDependenciesOptions.AutoDetect"/> which
        /// will check if knockoutjs is loaded and if it's not it will load it from CDN.
        /// </summary>
        /// <value>
        /// The load js dependencies value.
        /// </value>
        public LoadJsDependenciesOptions LoadJsDependencies { get; set; } = LoadJsDependenciesOptions.AutoDetect;
        /// <summary>
        /// Gets or sets is user admin moderator check. Function that checks if currently logged in user is comments moderator.
        /// </summary>
        /// <value>
        /// Is user admin moderator check.
        /// </value>
        public Func<HttpContext, bool> IsUserAdminModeratorCheck { get; set; } =
            ctx => ctx.User.Identity.IsAuthenticated && ctx.User.IsInRole("commentsmod");

        /// <summary>
        /// Gets or sets a value indicating whether comment approval is needed before it becomes visible..
        /// </summary>
        /// <value>
        /// <c>true</c> if comment approval is required before it's visible; otherwise, <c>false</c>.
        /// </value>
        public bool RequireCommentApproval { get; set; } = false;

        /// <summary>
        /// Gets or sets value indicating if query part of URL should be included in detecting page URL for comment posting and loading. 
        /// Default is false.
        /// </summary>
        /// <value>
        /// The include query in URL.
        /// </value>
        public bool IncludeQueryInUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether post comment form should be visible when page loads. 
        /// If false, user will have to click on post comment label to see the form. Default is true.
        /// </summary>
        /// <value>
        /// <c>true</c> if comment posting form should be visible on page load; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayPostCommentDivOnLoad { get; set; } = true;

        /// <summary>
        /// Gets or sets the Markdig pipeline. Markdig is library which is used for parsing markdown and converting it to HTML.
        /// Default value is null, which means use most basic pipeline with disabled HTML (removes user-entered HTML tags from comments).
        /// </summary>
        /// <value>
        /// The Markdig pipeline.
        /// </value>
        public MarkdownPipeline MarkdigPipeline { get; set; }
    }
}
