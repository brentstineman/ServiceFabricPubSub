using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace Administration.Controllers
{
    [ServiceRequestActionFilter]
    public class TenantsController : ApiController
    {

        private readonly TimeSpan operationTimeout = TimeSpan.FromSeconds(20);
        static FabricClient fabricClient = new FabricClient();
        private readonly IApplicationLifetime appLifetime;

        public async Task<string> Get(string ApplicationType, string TenantName, string AppVersion)
        {

            if (!isValidTenantName(TenantName)) return "500 - Invalid TenantName.";

            ApplicationDescription application = new ApplicationDescription(
                new Uri("fabric:/" + TenantName),
                ApplicationType,
                AppVersion);
            
            //await fabricClient.ApplicationManager.CreateApplicationAsync(application, 
            //    operationTimeout, appLifetime.ApplicationStopping);

            await fabricClient.ApplicationManager.CreateApplicationAsync(application);

            return "200";

        }

        public async Task<string> Get(string TenantName, string AppVersion)
        {

            if(!isValidTenantName(TenantName)) return "500 - Invalid TenantName.";

            string ApplicationType = "TenantApplicationType";

            ApplicationDescription application = new ApplicationDescription(
                new Uri("fabric:/" + TenantName),
                ApplicationType,
                AppVersion);

            //await fabricClient.ApplicationManager.CreateApplicationAsync(application,
            //    operationTimeout, appLifetime.ApplicationStopping);

            await fabricClient.ApplicationManager.CreateApplicationAsync(application);

            return "200";

        }

        private bool isValidTenantName(string TenantName)
        {
            Regex r1 = new Regex("^[A-Za-z0-9]{8,15}$");
            Match match = r1.Match(TenantName);

            return match.Success;
            
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
