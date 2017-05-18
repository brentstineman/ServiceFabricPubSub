using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SubscriberService.Controllers
{
    [Route("api")]
    public class SubscriberController : Controller
    {
        // GET api/values/5
        [HttpGet]
        public object Pop()
        {
            return "value";
        }
    }
}
