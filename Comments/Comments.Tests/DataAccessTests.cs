using Comments.Contracts;
using Comments.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace Comments.Tests
{
    public class DataAccessTests
    {
        [Fact]
        public void CanSelectCount()
        {
            string file = null;
            using (var connection = GetDb())
            {
                file = connection.ConnectionString.Split('=')[1];
                var dataAccess = new SqliteDataAccess(connection);
                dataAccess.Initialize();
                int count = dataAccess.GetCommentsCount("/");
                dataAccess.Dispose();
                Assert.Equal(0, count);
            }
            TryDelete(file);
        }

        [Fact]
        public void CanInsertAndSelect()
        {
            string file = null;
            using (var connection = GetDb())
            {
                file = connection.ConnectionString.Split('=')[1];
                var dataAccess = new SqliteDataAccess(connection);
                dataAccess.Initialize();

                var model = GetTestModel();
                var response = dataAccess.PostComment(model);
                Assert.True(response != null && response.Id > 0);
                AssertModels(model, response);
                Assert.NotEqual(response.StaticId, Guid.Empty);

                var model2 = GetTestModel();
                model2.CommentHistory = null;
                model2.ReplayToCommentId = null;
                var response2 = dataAccess.PostComment(model2);
                Assert.True(response2 != null && response2.Id > 0);
                AssertModels(model2, response2);

                int count = dataAccess.GetCommentsCount(model.PageUrl);
                Assert.Equal(2, count);

                var comments = dataAccess.GetCommentsForPage(model.PageUrl, 0, 5).ToArray();
                Assert.Equal(2, comments.Length);

                AssertModels(model, comments[0]);
                AssertModels(model2, comments[1]);

                var deletedComment = dataAccess.DeleteComment(comments[0].StaticId, "-");
                Assert.True(deletedComment.Deleted);
                Assert.Equal("-", deletedComment.ReasonForDeleting);
                Assert.Equal("Comment is deleted", deletedComment.CommentContentRendered);

                dataAccess.Dispose();
            }
            TryDelete(file);
        }

        private IDbConnection GetDb()
        {
            var connection = new SqliteConnection($"Data Source=d:\\Temp\\test_comments_{Guid.NewGuid().ToString("N")}.sqlite");
            connection.Open();
            return connection;
        }

        private CommentModel GetTestModel()
            => new CommentModel
            {
                CommentContentRendered = "rendered",
                CommentContentSource = "source",
                CommentHistory = "history",
                Deleted = true,
                ReasonForDeleting = "reason",
                PageUrl = "url",
                PostedByMod = true,
                PosterEmail = "email",
                PosterEmailHash = "hash",
                PosterName = "name",
                PostTime = DateTime.Parse("2017-07-22 14:59:44"),
                ReplayToCommentId = 4,
                StaticId = Guid.Parse("16d55167-53b8-47b9-afa0-650280dd8cb9")
            };

        private void TryDelete(string file, int tryCount = 0)
        {
            if (tryCount > 2) throw new Exception("Failed to delete file");
            Thread.Sleep(tryCount * 1000);
            try
            {
                File.Delete(file);
            }
            catch
            {
                TryDelete(file, tryCount + 1);
            }
        }

        private void AssertModels(CommentModel m1, CommentModel m2)
        {
            Assert.Equal(m1.CommentContentRendered, m2.CommentContentRendered);
            Assert.Equal(m1.CommentContentSource, m2.CommentContentSource);
            Assert.Equal(m1.CommentHistory, m2.CommentHistory);
            Assert.Equal(m1.Deleted, m2.Deleted);
            Assert.Equal(m1.PageUrl, m2.PageUrl);
            Assert.Equal(m1.PostedByMod, m2.PostedByMod);
            Assert.Equal(m1.PosterEmail, m2.PosterEmail);
            Assert.Equal(m1.PosterEmailHash, m2.PosterEmailHash);
            Assert.Equal(m1.PosterName, m2.PosterName);
            Assert.Equal(m1.PostTime, m2.PostTime);
            Assert.Equal(m1.ReasonForDeleting, m2.ReasonForDeleting);
            Assert.Equal(m1.ReplayToCommentId, m2.ReplayToCommentId);
        }
    }
}
