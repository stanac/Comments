using System;

namespace Comments.Contracts
{
    class DeleteCommentModel
    {
        public Guid StaticId { get; set; }
        public string ReasonForDeleting { get; set; }
    }
}
