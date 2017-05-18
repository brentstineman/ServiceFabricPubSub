using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Query;
using System.IO;
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
        #region GLOBAL 

        private readonly TimeSpan operationTimeout = TimeSpan.FromSeconds(20);
        static FabricClient fabricClient = new FabricClient();

        private const string APPLICATION_NAME = "fabric:/FrontEnd";
        private readonly string APPLICATIONTYPE_NAME = "TenantApplicationType";

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
            // Need to implement better error handler

            if (!IsValidTenantName(TenantName)) throw new HttpResponseException(HttpStatusCode.BadRequest);

            ApplicationList applicationList = await fabricClient.QueryManager.GetApplicationListAsync(new Uri("fabric:/" + TenantName));

            if (applicationList.Count > 0) return HttpStatusCode.OK;

            ApplicationDescription applicationDescripriont = new ApplicationDescription(
                new Uri("fabric:/" + TenantName), APPLICATIONTYPE_NAME, AppVersion);

            await fabricClient.ApplicationManager.CreateApplicationAsync(applicationDescripriont);

            try
            {
                LogOnTeams("Tenant " + TenantName + " created.");
            }
            catch (Exception)
            {

                // Implements other log option when teams not available 
            }

            return HttpStatusCode.OK;

        }

        /// <summary>
        /// Send a message to a connector on Microsoft Teams. 
        /// </summary>
        /// <param name="message"></param>
        private async void LogOnTeams(string message)
        {
            // This connector only work on development environment.
            // Needs to improve to produtcion

            string TeamsUrl = "https://outlook.office.com/webhook/5f099c80-76ae-42d2-b0b4-2fb446d03858";
            string WebhookId = "72f988bf-86f1-41af-91ab-2d7cd011db47/IncomingWebhook/bcdb3f4c5787401bbf4b7869a20aead5/b1dfd638-2319-4e25-a2b1-fcbfd16a8e09";
            string TeamsConnectorUri = TeamsUrl + "@" + WebhookId;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(TeamsConnectorUri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"text\": \"" + message + "\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse =  (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }

    }
}
