using System;
using System.Collections.Generic;

namespace Comments.Contracts
{
    public interface IDataAccess
    {
        IEnumerable<CommentModel> GetCommentsForPage(string pageUrl, int start, int end);
        int GetCommentsCount(string pageUrl);
        CommentModel PostComment(CommentModel model);
        void DeleteComment(Guid staticId, string reasonForDeleting);
        void Initialize();
    }
}
