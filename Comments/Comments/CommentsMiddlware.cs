using Comments.Actions;
using Comments.Contracts;
using Comments.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comments
{
    public class CommentsMiddlware
    {
        private readonly RequestDelegate _next;
        private readonly CommentsOptions _options;
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly IReadOnlyList<ActionHandler> _actionHandlers;

        public CommentsMiddlware(RequestDelegate next, CommentsOptions options)
        {
            _options = options ?? new CommentsOptions();
            _next = next;
            var dataAccess = new SqliteDataAccess(options.SqliteDbFilePath);
            dataAccess.Initialize();
            dataAccess.Dispose();
            _dataAccessFact = () => new SqliteDataAccess(options.SqliteDbFilePath);
            _actionHandlers = GetActions().ToList();
        }

        public async Task Invoke(HttpContext ctx)
        {
            if (ctx.Request.Path.StartsWithSegments(_options.BaseUrl))
            {
                var action = _actionHandlers.FirstOrDefault(x => x.ShouldHandleRequest(ctx.Request));
                if (action != null)
                {
                    await action.HandleRequest(ctx);
                    return;
                }
            }

            if (_next != null)
            {
                await _next.Invoke(ctx);
            }
        }

        private IEnumerable<ActionHandler> GetActions()
        {
            var converter = new CommentsConverter(_options.MarkdigPipeline);
            List<string> knownAssets = new List<string>
            {
                "/loader.js",
                "/comments.js",
                // "/comments.min.js",
                "/view.html",
                "/style.css"
            };

            yield return new AssetLoadingActionHandlerFactory(_dataAccessFact, _options, knownAssets)
                .GetActionHandler();
            yield return new CommentsCountActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            yield return new PostCommentActionHandlerFactory(_dataAccessFact, _options, converter)
                .GetActionHandler();
            yield return new GetCommentsActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            yield return new IsUserCommentModActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            yield return new DeleteCommentActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            yield return new PreviewMarkdownActionHandlerFactory(_dataAccessFact, _options, converter)
                .GetActionHandler();
            yield return new ApproveCommentActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
        }
    }
}
