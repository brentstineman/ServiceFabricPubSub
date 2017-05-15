using System.Collections.Generic;
using System.Web.Http;

namespace Administration.Controllers
{
    [ServiceRequestActionFilter]
    public class TenantsController : ApiController
    {
        // GET api/tenants 
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/tenants/5 
        public string Get(int id)
        {
            return "value";
        }

        // POST api/tenants 
        public void Post([FromBody]string value)
        {
            // creates a new instance of the tenant application
        }

        // PUT api/tenants/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/tenants/5 
        public void Delete(int id)
        {
        }
    }
}
