using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lightchain.httpserver
{
    public interface IController
    {
        Task ProcessAsync(HttpContext context);
    }
}
