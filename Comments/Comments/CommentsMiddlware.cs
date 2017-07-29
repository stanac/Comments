using Microsoft.AspNetCore.Http;
using System;

namespace Comments
{
    public class CommentsMiddlware
    {
        private RequestDelegate _next;

        public CommentsMiddlware(RequestDelegate next)
        {
            _next = next;

        }
    }
}
