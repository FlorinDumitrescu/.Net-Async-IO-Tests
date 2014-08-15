using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace LongRunningRequest.Controllers
{
    public class LongRunningController : ApiController
    {
        public async Task<string> Get()
        {
            await Task.Delay(500);
            return "Hello world";
        }

        //public string Get()
        //{
        //    System.Threading.Thread.Sleep(500);
        //    return "Hello world";
        //}
    }
}
