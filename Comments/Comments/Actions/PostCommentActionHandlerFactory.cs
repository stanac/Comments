using Comments.Contracts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class PostCommentActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;
        private readonly ICommentsConverter _mardownParser;

        public PostCommentActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options, ICommentsConverter mardownParser)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataAccessFact = dataAccessFact ?? throw new ArgumentNullException(nameof(dataAccessFact));
            _mardownParser = mardownParser ?? throw new ArgumentNullException(nameof(mardownParser));
        }

        public ActionHandler GetActionHandler()
        {
            return new ActionHandler
            {
                RequestMethod = "post",
                RequestUrl = (_options.BaseUrl + "/create").NormalizePath(),
                HandleRequest = HandleRequest
            };
        }

        public async Task HandleRequest(HttpContext ctx)
        {
            try
            {
                string json = ctx.Request.ReadBodyAsString();
                CommentModel comment = JsonConvert.DeserializeObject<CommentModel>(json);
                
                comment.SetEmailHash();
                comment.PostTime = DateTime.UtcNow;
                comment.PageUrl = comment.PageUrl.NormalizePath();
                if (comment.CommentContentSource.Length > _options.CommentSourceMaxLength)
                {
                    await ctx.Response.WriteResponse($"Comment has exceeded maximum length of {_options.CommentSourceMaxLength} characters.", "text/plain", 400);
                    return;
                }
                comment.Approved = !_options.RequireCommentApproval;
                if (!comment.Approved)
                {
                    comment.Approved = _options.IsUserAdminModeratorCheck(ctx); // admins don't require approval for comments
                }
                comment.PostedByMod = _options.IsUserAdminModeratorCheck(ctx);
                if (comment.IsMarkdown)
                {
                    comment.CommentContentRendered = _mardownParser.ConvertToHtml(comment.CommentContentSource);
                }
                else
                {
                    comment.CommentContentSource = WebUtility.HtmlEncode(comment.CommentContentSource);
                    comment.CommentContentSource = comment.CommentContentSource.Replace("\n", " <br /> ");
                    comment.CommentContentRendered = comment.CommentContentSource;
                }
                CommentModel response = null;
                using (var dataAccess = _dataAccessFact())
                {
                    response = dataAccess.PostComment(comment);
                }
                string responseJson = JsonConvert.SerializeObject(response);
                await ctx.Response.WriteResponse(responseJson, "application/json", 201);
            }
			catch (Exception ex)
            {
                throw new Exception("Failed to post comment", ex);
            }
        }
    }
}
