using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Administration.Controllers
{
    [ServiceRequestActionFilter]
    public class TenantsController : ApiController
    {

        private readonly TimeSpan operationTimeout = TimeSpan.FromSeconds(20);
        static FabricClient fabricClient = new FabricClient();
        private readonly IApplicationLifetime appLifetime;

        [HttpGet]
        [Route("api/Tenants")]
        public async Task<HttpStatusCode> Get(string ApplicationType, string TenantName, string AppVersion)
        {

            if (!IsValidTenantName(TenantName)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            ApplicationDescription application = new ApplicationDescription(
                new Uri("fabric:/" + TenantName),
                ApplicationType,
                AppVersion);

            await fabricClient.ApplicationManager.CreateApplicationAsync(application);

            return HttpStatusCode.OK;
        }

        [HttpGet]
        [Route("/api/tenants")]
        public async Task<HttpStatusCode> Get(string TenantName, string AppVersion)
        {

            if (!IsValidTenantName(TenantName)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            string ApplicationType = "TenantApplicationType";

            ApplicationDescription application = new ApplicationDescription(
                new Uri("fabric:/" + TenantName),
                ApplicationType,
                AppVersion);

            await fabricClient.ApplicationManager.CreateApplicationAsync(application);

            return HttpStatusCode.OK;
        }

        private bool IsValidTenantName(string TenantName)
        {
            Regex r1 = new Regex("^[A-Za-z0-9]{8,15}$");
            Match match = r1.Match(TenantName);

            return match.Success;
        }
    }
}
