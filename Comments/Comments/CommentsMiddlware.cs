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
            var dataAccess = new SqliteDataAccess(options.FilePath);
            dataAccess.Initialize();
            dataAccess.Dispose();
            _dataAccessFact = () => new SqliteDataAccess(options.FilePath);
            _actionHandlers = GetActions().ToList();
        }

        public async Task Invoke(HttpContext ctx)
        {
            var action = _actionHandlers.FirstOrDefault(x => x.ShouldHandleRequest(ctx.Request));
            if (action != null)
            {
                await action.HandleRequest(ctx);
            }
            else if (_next != null)
            {
                await _next.Invoke(ctx);
            }
        }

        private IEnumerable<ActionHandler> GetActions()
        {
            yield return new CommentsCountActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            yield return new PostCommentActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            yield return new GetCommentsActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            List<string> knownAssets = new List<string>
            {
                "/loader.js",
                "/comments.js"
            };
            yield return new AssetLoadingActionHandlerFactory(_dataAccessFact, _options, knownAssets)
                .GetActionHandler();
            yield return new IsUserCommentModActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            yield return new DeleteCommentActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
            yield return new PreviewMarkdownActionHandlerFactory(_dataAccessFact, _options)
                .GetActionHandler();
        }
    }
}
