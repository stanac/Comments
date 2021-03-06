﻿using Comments.Contracts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Comments.Actions
{
    class IsUserCommentModActionHandlerFactory
    {
        private readonly Func<IDataAccess> _dataAccessFact;
        private readonly CommentsOptions _options;

        public IsUserCommentModActionHandlerFactory(Func<IDataAccess> dataAccessFact, CommentsOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dataAccessFact = dataAccessFact ?? throw new ArgumentNullException(nameof(dataAccessFact));
        }

        public ActionHandler GetActionHandler()
        {
            return new ActionHandler
            {
                RequestMethod = "get",
                RequestUrl = (_options.BaseUrl + "/is-admin").NormalizePath(),
                HandleRequest = HandleRequest
            };
        }

        public async Task HandleRequest(HttpContext ctx)
        {
            bool isAdmin = _options.IsUserAdminModeratorCheck(ctx);
            int result = isAdmin ? 1 : 0;
            await ctx.Response.WriteResponse(result.ToString(), "application/json", 200);
        }
    }
}
