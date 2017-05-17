using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Query;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Administration.Controllers
{
    [ServiceRequestActionFilter]
    public class TenantsController : ApiController
    {
#       region GLOBAL 

        private readonly TimeSpan operationTimeout = TimeSpan.FromSeconds(20);
        static FabricClient fabricClient = new FabricClient();

        private const string APPLICATION_NAME = "fabric:/FrontEnd";
        private readonly string APPLICATIONTYPE_NAME = "PubSubTransactionPoCType"; //"TenantApplicationType";
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TenantName"></param>
        /// <param name="AppVersion"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("/api/tenants")]
        public async Task<HttpStatusCode> GetTenant(string TenantName, string AppVersion)
        {
            if (!IsValidTenantName(TenantName)) throw new HttpResponseException(HttpStatusCode.BadRequest);
            
            ApplicationList applicationList = await fabricClient.QueryManager.GetApplicationListAsync(new Uri("fabric:/" + TenantName));

            if (applicationList.Count > 0) return HttpStatusCode.OK;

            ApplicationDescription applicationDescripriont = new ApplicationDescription(
                new Uri("fabric:/" + TenantName), APPLICATIONTYPE_NAME, AppVersion);
                         
            await fabricClient.ApplicationManager.CreateApplicationAsync(applicationDescripriont);

            return HttpStatusCode.OK; 
        }

        /// <summary>
        /// Validate the defined pattern to Tenant Name ("^[A-Za-z0-9]{8,15}$")
        /// </summary>
        /// <param name="TenantName"></param>
        /// <returns></returns>
        private bool IsValidTenantName(string TenantName)
        {
            Regex r1 = new Regex("^[A-Za-z0-9]{8,15}$");
            Match match = r1.Match(TenantName);

            return match.Success;
        }
    }
}
