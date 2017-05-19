using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FrontEndHelper;

namespace Administration.Controllers
{
    [RequestAuthorization]
    [ServiceRequestActionFilter]
    public class TopicsController : ApiController
    {

        private const string TenantApplicationAppName = "TenantApplication";
        private const string TenantApplicationAdminServiceName = "Admin";

        [HttpPut()]
        public async Task<string> CreateTopic(string TenantName, string TopicName)
        {
            var key = HttpContext.Current.Request.Headers.GetValues("x-request-key").FirstOrDefault();


            var client = new HttpClient();
            
            Uri uri = new Uri("http://localhost:19081/TenantApplication/Admin/api/topics");
            HttpResponseMessage response = await client.PutAsJsonAsync(uri, TopicName);
            
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();

        }

        
    }
}
