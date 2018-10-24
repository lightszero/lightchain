using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lightchain.httpserver
{
    public class ActionController : IController
    {
        public ActionController(httpserver.onProcessHttp action)
        {
            this.action = action;
        }
        public async Task ProcessAsync(HttpContext context)
        {
            await action(context);
        }
        httpserver.onProcessHttp action;
    }
}
