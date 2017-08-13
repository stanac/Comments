using System;

namespace Comments.Contracts
{
    public class CommentModel
    {
        public long Id { get; set; }
        public string PageUrl { get; set; }
        public long? ReplayToCommentId { get; set; }
        public string PosterName { get; set; }
        public string PosterEmail { get; set; }
        public string PosterEmailHash { get; set; }
        public string CommentContentSource { get; set; }
        public string CommentContentRendered { get; set; }
        public DateTime PostTime { get; set; }
        public bool Approved { get; set; }
        public bool Deleted { get; set; }
        public bool PostedByMod { get; set; }
        public string ReasonForDeleting { get; set; }
        public Guid StaticId { get; set; }
        public string CommentHistory { get; set; }
    }
}
