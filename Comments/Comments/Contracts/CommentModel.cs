using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
        public bool IsMarkdown { get; set; }
        
        // not mapped data
        public CommentModel[] ChildComments { get; set; }
        public string ReplyToPersonName { get; set; }

        public void SetEmailHash()
        {
            string value = PosterEmail.Trim().ToLower();
            byte[] data = Encoding.UTF8.GetBytes(value);
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                PosterEmailHash = string.Join("", hash.Select(c => ((int)c).ToString("X2"))).ToLower();
            }
        }
    }
}
