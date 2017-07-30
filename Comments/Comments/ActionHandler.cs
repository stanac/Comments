using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Comments
{
    class ActionHandler
    {
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public Func<HttpContext, Task> HandleRequest { get; set; }
        public virtual bool ShouldHandleRequest(HttpRequest request)
        {
            return request.Path.ToString().ToLower() == RequestUrl.ToLower() && request.Method.ToLower() == RequestMethod.ToLower();
        }

    }
}
