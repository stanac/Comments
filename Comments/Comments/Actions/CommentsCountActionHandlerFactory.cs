using Comments.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class CommentsCountActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;

        public CommentsCountActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataAccessFact = dataAccessFact ?? throw new ArgumentNullException(nameof(dataAccessFact));
        }

        public ActionHandler GetActionHandler()
        {
            return new ActionHandler
            {
                RequestMethod = "get",
                RequestUrl = (_options.BaseUrl + "/count").NormalizePath(),
                HandleRequest = HandleRequest
            };
        }

        public async Task HandleRequest(HttpContext ctx)
        {
            string response = _options.NoCommentsTemplate;
            if (ctx.Request.Query.ContainsKey("url"))
            {
                string url = ctx.Request.Query["url"].ToString().NormalizePath();
                int count = 0;
                using (var dataAccess = _dataAccessFact())
                {
                    count = dataAccess.GetCommentsCount(url);
                }
                if (_options.CommentCountFormater != null)
                {
                    response = _options.CommentCountFormater(count);
                }
                else
                {
                    if (count == 0) response = _options.NoCommentsTemplate;
                    else if (count == 1) response = _options.OneCommentTemplate;
                    else response = _options.MoreThanOneCommentTemplate.Replace("{count}", count.ToString());
                }
            }
            await ctx.Response.WriteResponse(response, "text/plain", 200);
        }
    }
}
