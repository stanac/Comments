using Comments.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class GetCommentsActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;

        public GetCommentsActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataAccessFact = dataAccessFact ?? throw new ArgumentNullException(nameof(dataAccessFact));
        }

        public ActionHandler GetActionHandler()
        {
            return new ActionHandler
            {
                RequestMethod = "get",
                RequestUrl = (_options.BaseUrl + "/all-comments").NormalizePath(),
                HandleRequest = HandleRequest
            };
        }

        public async Task HandleRequest(HttpContext ctx)
        {
            try
            {
                string response = null;
                StringValues url;
                StringValues start;
                StringValues count;
                if (ctx.Request.Query.TryGetValue("url", out url))
                {
                    if (!ctx.Request.Query.TryGetValue("start", out start))
                    {
                        start = "0";
                    }
                    if (!ctx.Request.Query.TryGetValue("count", out count))
                    {
                        count = "5000";
                    }
                    string theUrl = url;
                    bool includeNotApproved = _options.IsUserAdminModeratorCheck(ctx);
                    var comments = _dataAccessFact().GetCommentsForPage(
                        theUrl.NormalizePath(), 
                        int.Parse(start), 
                        int.Parse(count),
                        includeNotApproved
                        ).ToArray();
                    foreach (var c in comments)
                    {
                        c.CommentContentSource = "";
                    }
                    response = JsonConvert.SerializeObject(comments);
                }

                await ctx.Response.WriteResponse(response, "application/json", 200);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get comments", ex);
            }
        }
    }
}
