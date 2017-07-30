using Comments.Contracts;
using Markdig;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class PostCommentActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;

        public PostCommentActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataAccessFact = dataAccessFact ?? throw new ArgumentNullException(nameof(dataAccessFact));

        }

        public ActionHandler GetActionHandler()
        {
            return new ActionHandler
            {
                RequestMethod = "post",
                RequestUrl = _options.BaseUrl.NormalizePath(),
                HandleRequest = HandleRequest
            };
        }

        public async Task HandleRequest(HttpContext ctx)
        {
            try
            {
                string json = ctx.Request.ReadBodyAsString();
                var comment = JsonConvert.DeserializeObject<CommentModel>(json);
                if (comment.CommentContentSource.Length > _options.CommentSourceMaxLength)
                {
                    await ctx.Response.WriteResponse($"Comment has exceeded maximum length of {_options.CommentSourceMaxLength} characters.", "text/plain", 400);
                    return;
                }
                comment.PostedByMod = _options.IsUserAdminModeratorCheck(ctx);
                comment.CommentContentRendered = Markdown.ToHtml(comment.CommentContentSource);
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
