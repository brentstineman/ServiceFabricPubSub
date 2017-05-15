using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RequestRouterService.Controllers
{
    public class RequestController : ApiController
    {
        // PUT api/tenantId/topicName
        public HttpResponseMessage Put(string tenantId, string topicName)
        {
            // TODO: Do work!

            HttpResponseMessage responseMessage = new HttpResponseMessage {StatusCode = HttpStatusCode.Accepted};

            return responseMessage;
        }
    }
}
