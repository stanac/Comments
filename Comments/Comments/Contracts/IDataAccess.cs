using System;
using System.Collections.Generic;

namespace Comments.Contracts
{
    public interface IDataAccess: IDisposable
    {
        IEnumerable<CommentModel> GetCommentsForPage(string pageUrl, int start, int end);
        int GetCommentsCount(string pageUrl);
        CommentModel PostComment(CommentModel model);
        CommentModel DeleteComment(Guid staticId, string reasonForDeleting);
        CommentModel ApproveComment(Guid staticId);
        void Initialize();
    }
}
