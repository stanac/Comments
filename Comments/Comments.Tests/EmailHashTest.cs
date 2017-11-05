using Comments.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Comments.Tests
{
    public class EmailHashTest
    {
        [Fact]
        public void EmailHashingWorks()
        {
            var comment = new CommentModel
            {
                PosterEmail = "  MyEmailAddress@example.com "
            };
            comment.SetEmailHash();
            Assert.Equal("0bc83cb571cd1c50ba6f3e8a78ef1346", comment.PosterEmailHash);
        }
    }
}
