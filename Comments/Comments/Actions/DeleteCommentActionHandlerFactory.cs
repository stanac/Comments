using Comments.Contracts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class DeleteCommentActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;

        public DeleteCommentActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataAccessFact = dataAccessFact ?? throw new ArgumentNullException(nameof(dataAccessFact));
        }

        public ActionHandler GetActionHandler()
        {
            return new ActionHandler
            {
                RequestMethod = "post",
                RequestUrl = (_options.BaseUrl + "/delete").NormalizePath(),
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
                DeleteCommentModel deleteRequest = JsonConvert.DeserializeObject<DeleteCommentModel>(ctx.Request.ReadBodyAsString());
                CommentModel deletedComment = null;
                using (var dataAccess = _dataAccessFact())
                {
                    deletedComment = dataAccess.DeleteComment(deleteRequest.StaticId, deleteRequest.ReasonForDeleting);
                }
                string response = JsonConvert.SerializeObject(deletedComment);
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
