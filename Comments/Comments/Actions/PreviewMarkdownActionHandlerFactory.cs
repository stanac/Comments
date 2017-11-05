using Comments.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class PreviewMarkdownActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;
        private readonly ICommentsConverter _mardownParser;

        public PreviewMarkdownActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options, ICommentsConverter mardownParser)
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
                RequestUrl = (_options.BaseUrl + "/preview").NormalizePath(),
                HandleRequest = HandleRequest
            };
        }

        public async Task HandleRequest(HttpContext ctx)
        {
            try
            {
                string text = ctx.Request.ReadBodyAsString();
                if (text.Length > _options.CommentSourceMaxLength)
                {
                    await ctx.Response.WriteResponse($"Comment has exceeded maximum length of {_options.CommentSourceMaxLength} characters.", "text/plain", 400);
                    return;
                }
                var rendered = _mardownParser.ConvertToHtml(text);
                await ctx.Response.WriteResponse(rendered, "text/html", 200);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to preview comment", ex);
            }
        }
    }
}
