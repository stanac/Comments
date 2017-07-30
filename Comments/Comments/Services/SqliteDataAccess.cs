using Comments.Contracts;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Comments.Services
{
    public class SqliteDataAccess : IDataAccess, IDisposable
    {
        private readonly IDbConnection _connection;

        public SqliteDataAccess(): this("comments.sqlite") { }

        public SqliteDataAccess(string filePath): this(new SqliteConnection($"Data Source={filePath};")) { }

        public SqliteDataAccess(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public IEnumerable<CommentModel> GetCommentsForPage(string pageUrl, int start, int count)
        {
            pageUrl = NormalizeUrl(pageUrl);
            string sql = $"SELECT * FROM Comment WHERE PageUrl = $pageUrl ORDER BY Id ASC LIMIT {count} OFFSET {start};";
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            var param = cmd.CreateParameter();
            cmd.AddParamWithValue("$pageUrl", pageUrl);
            var models = ReadCommentModels(cmd).ToArray();
            cmd.Dispose();
            _connection.Close();
            return models;
        }

        public int GetCommentsCount(string pageUrl)
        {
            string sql = "SELECT COUNT(1) FROM Comment WHERE PageUrl = $pageUrl;";
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            var param = cmd.CreateParameter();
            param.ParameterName = "$pageUrl";
            param.Value = pageUrl;
            cmd.Parameters.Add(param);
            if (_connection.State != ConnectionState.Open) _connection.Open();
            object result = cmd.ExecuteScalar();
            cmd.Dispose();
            _connection.Close();
            return (int)(long)result;
        }

        public CommentModel DeleteComment(Guid staticId, string reasonForDeleting)
        {
            // TODO: get deleted comment content from CommentsOptions
            string staticIdValue = staticId.ToString("N");

            string sql = $"UPDATE Comment SET Deleted = 1, CommentContentRendered = 'Comment is deleted', ReasonForDeleting = $reason WHERE StaticId = '{staticIdValue}';";
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParamWithValue("$reason", reasonForDeleting);
            if (_connection.State != ConnectionState.Open) _connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            
            string sqlRead = $"SELECT * FROM Comment WHERE StaticId = '{staticIdValue}';";
            var readCmd = _connection.CreateCommand();
            cmd.CommandText = sqlRead;
            var models = ReadCommentModels(cmd).ToArray();
            cmd.Dispose();

            _connection.Close();

            if (models.Length != 1)
            {
                throw new Exception($"Failed to get comment with id: '{staticIdValue}' after deleting");
            }
            return models[0];
        }

        public CommentModel PostComment(CommentModel model)
        {
            model.StaticId = Guid.NewGuid();
            string staticId = model.StaticId.ToString("N");
            
            string sql = $@"
                INSERT INTO Comment (
                    PageUrl,
                    ReplayToCommentId,
                    PosterName,
                    PosterEmail,
                    PosterEmailHash,
                    CommentContentSource,
                    CommentContentRendered,
                    PostTime,
                    Deleted,
                    ReasonForDeleting,
                    PostedByMod,
                    StaticId,
                    CommentHistory )
                VALUES (
                    $PageUrl,
                    {(model.ReplayToCommentId.HasValue ? model.ReplayToCommentId.ToString() : "NULL")},
                    $PosterName,
                    $PosterEmail,
                    $PosterEmailHash,
                    $CommentContentSource,
                    $CommentContentRendered,
                    '{DateTimeToString(model.PostTime)}',
                    '{(model.Deleted ? 1 : 0)}',
                    $ReasonForDeleting,
                    {(model.PostedByMod ? 1 : 0)},
                    '{staticId}',
                    $CommentHistory )
                    ;
                ";

            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.AddParamWithValue("$PageUrl", model.PageUrl)
                .AddParamWithValue("$PosterName", model.PosterName)
                .AddParamWithValue("$PosterEmail", model.PosterEmail)
                .AddParamWithValue("$PosterEmailHash", model.PosterEmailHash)
                .AddParamWithValue("$CommentContentSource", model.CommentContentSource)
                .AddParamWithValue("CommentContentRendered", model.CommentContentRendered)
                .AddParamWithValue("ReasonForDeleting", model.ReasonForDeleting)
                .AddParamWithValue("CommentHistory", model.CommentHistory);
            if (_connection.State != ConnectionState.Open) _connection.Open();
            cmd.ExecuteNonQuery();

            string sqlRead = $"SELECT * FROM Comment WHERE StaticId = '{staticId}';";
            var readCmd = _connection.CreateCommand();
            cmd.CommandText = sqlRead;
            var models = ReadCommentModels(cmd).ToArray();
            cmd.Dispose();

            _connection.Close();

            if (models.Count() != 1)
            {
                throw new Exception("failed to read inserted model");
            }

            return models.First();
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
            _connection.Dispose();
        }
        
        public void Initialize()
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Comment(
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    PageUrl TEXT NOT NULL,
                    ReplayToCommentId BIGINT,
                    PosterName TEXT NOT NULL,
                    PosterEmail TEXT NOT NULL,
                    PosterEmailHash TEXT NOT NULL,
                    CommentContentSource TEXT NOT NULL,
                    CommentContentRendered TEXT NOT NULL,
                    PostTime DATETIME NOT NULL,
                    Deleted BIT NOT NULL,
                    ReasonForDeleting TEXT,
                    PostedByMod BIT NOT NULL,
                    CommentHistory TEXT NULL,
                    StaticId TEXT NOT NULL UNIQUE
                );

                CREATE INDEX IF NOT EXISTS IX_Comment_PageUrl ON Comment(PageUrl);

                CREATE UNIQUE INDEX IF NOT EXISTS IX_Comment_StaticId ON Comment(StaticId);
                ";

            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            if (_connection.State != ConnectionState.Open) _connection.Open();
            cmd.ExecuteNonQuery();
            _connection.Close();
            cmd.Dispose();
        }

        private string NormalizeUrl(string url)
        {
            url = url ?? "";
            url = url.TrimEnd('/').ToLower();
            return url;
        }

        private IEnumerable<CommentModel> ReadCommentModels(IDbCommand cmd)
        {
            if (_connection.State != ConnectionState.Open) _connection.Open();
            var r = cmd.ExecuteReader();
            try
            {
                Func<IDataReader, CommentModel> readModel = reader =>
                    new CommentModel
                    {
                        CommentContentRendered = reader.Get<string>("CommentContentRendered"),
                        CommentContentSource = reader.Get<string>("CommentContentSource"),
                        Deleted = reader.Get<bool>("Deleted"),
                        Id = reader.Get<int>("Id"),
                        PageUrl = reader.Get<string>("PageUrl"),
                        PostedByMod = reader.Get<bool>("PostedByMod"),
                        PosterEmail = reader.Get<string>("PosterEmail"),
                        PosterEmailHash = reader.Get<string>("PosterEmailHash"),
                        PosterName = reader.Get<string>("PosterName"),
                        PostTime = reader.Get<DateTime>("PostTime"),
                        ReasonForDeleting = reader.Get<string>("ReasonForDeleting"),
                        ReplayToCommentId = reader.Get<long?>("ReplayToCommentId"),
                        StaticId = Guid.Parse(reader.Get<string>("StaticId")),
                        CommentHistory = reader.Get<string>("CommentHistory")
                    };

                while (r.Read())
                {
                    yield return readModel(r);
                }
            }
            finally
            {
                r.Close();
                r.Dispose();
            }
        }
        
        private string DateTimeToString(DateTime dt) => dt.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
