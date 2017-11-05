using Markdig;

namespace Comments
{
    internal class CommentsConverter : ICommentsConverter
    {
        private readonly MarkdownPipeline _pipeline;

        public CommentsConverter() : this(null) { }

        public CommentsConverter(MarkdownPipeline pipeline)
        {
            _pipeline = pipeline ?? new MarkdownPipelineBuilder().DisableHtml().Build();
        }

        public string ConvertToHtml(string markdown)
        {
            return Markdown.ToHtml(markdown ?? "", _pipeline);
        }
        
    }
}
