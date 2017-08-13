using Comments.Contracts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class ApproveCommentActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;

        public ApproveCommentActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataAccessFact = dataAccessFact ?? throw new ArgumentNullException(nameof(dataAccessFact));
        }

        public ActionHandler GetActionHandler()
        {
            return new ActionHandler
            {
                RequestMethod = "post",
                RequestUrl = (_options.BaseUrl + "/approve").NormalizePath(),
                HandleRequest = HandleRequest
            };
        }

        public async Task HandleRequest(HttpContext ctx)
        {
            try
            {
                if (!(await CheckIfUserIsAdmin(ctx)))
                {
                    return;
                }
                Guid staticId = Guid.Parse(ctx.Request.ReadBodyAsString());
                CommentModel approvedComment = null;
                using (var dataAccess = _dataAccessFact())
                {
                    approvedComment = dataAccess.ApproveComment(staticId);
                }
                string response = JsonConvert.SerializeObject(approvedComment);
                await ctx.Response.WriteResponse(response, "application/json", 200);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to mark comment as deleted.", ex);
            }
        }

        private async Task<bool> CheckIfUserIsAdmin(HttpContext ctx)
        {
            bool isAdmin = _options.IsUserAdminModeratorCheck(ctx);
            if (!isAdmin)
            {
                await ctx.Response.WriteResponse("user is not comments moderator", "text/plain", 403);
                return false;
            }
            return true;
        }
    }
}
