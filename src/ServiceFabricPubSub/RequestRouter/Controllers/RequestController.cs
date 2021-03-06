﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FrontEndHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Fabric;
using PubSubDotnetSDK;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using RequestRouter;

namespace RequestRouterService.Controllers
{
    [RequestAuthorization]
    public class RequestController : ApiController
    {
        private const string TenantApplicationAdminServiceName = "Admin";
        private const string TenantApplicationTopicServiceName = "topics";

        private static int _reverseProxyPort;

        public async Task<HttpResponseMessage> Post(string tenantId, string topicName, string message)
        {
            try
            {
                ServiceEventSource.Current.Message($"[Router] New Message received for {tenantId}/{topicName}: {message}");

                HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
                {
                    PortNumber = await FrontEndHelper.FrontEndHelper.GetReverseProxyPortAsync(),
                    ServiceName = $"{tenantId}/{TenantApplicationTopicServiceName}/{topicName}/api"
                };

                HttpResponseMessage msg;
                PubSubMessage pubsubMessage = new PubSubMessage() { Message = message };
                using (HttpClient httpClient = new HttpClient())
                {
                    string msgJson = JsonConvert.SerializeObject(pubsubMessage,
                       new JsonSerializerSettings
                       {
                           Formatting = Formatting.Indented
                       });
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                   
                    msg = await httpClient.PostAsync(
                        builder.Build(),
                        new StringContent(msgJson, Encoding.UTF8, "application/json")
                        );
                }

                HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                if (!msg.IsSuccessStatusCode)
                {
                    responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                }

                return responseMessage;

                //using remoting

                //var msg = new PubSubMessage() { Message = message };
                //var topicSvc = ServiceProxy.Create<ITopicService>(new Uri($"fabric:/{tenantId}/{TenantApplicationTopicServiceName}/{topicName}"));
                //await topicSvc.Push(msg);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        // POST api/tenantId/topicName
        //public async Task<HttpResponseMessage> Post(string tenantId, string topicName)
        //{
        //    HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        //    // Assuming the message body will contain the content for the data to put to the topic.
        //    string messageBody = await this.Request.Content.ReadAsStringAsync();

        //    HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
        //    {
        //        PortNumber = await GetReverseProxyPortAsync(),
        //        ServiceName = $"{tenantId}/{TenantApplicationTopicServiceName}/{topicName}/api/"
        //    };

        //    HttpResponseMessage topicResponseMessage;
        //    using (HttpClient httpClient = new HttpClient())
        //    {
        //        HttpContent postContent = new StringContent(messageBody);
        //        postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //        topicResponseMessage = await httpClient.PostAsync(builder.Build(), postContent);
        //    }

        //    if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
        //    {
        //        responseMessage.StatusCode = HttpStatusCode.Accepted;

        //        var msg = await topicResponseMessage.Content.ReadAsStringAsync();

        //        System.Diagnostics.Debug.WriteLine($"Received response of '{msg}'.");
        //    }
        //    else
        //    {
        //        responseMessage.StatusCode = topicResponseMessage?.StatusCode ?? HttpStatusCode.InternalServerError;
        //        responseMessage.ReasonPhrase = topicResponseMessage?.ReasonPhrase ?? "Internal error";
        //    }

        //    return responseMessage;
        //}

        // GET api/tenantId/topicName/subscriber
        public async Task<HttpResponseMessage> Get(string tenantId, string topicName, string subscriberName)
        {
            try
            {
                ServiceEventSource.Current.Message($"[Router] Message read for {tenantId}/{topicName}/{subscriberName}");

                HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
                {
                    PortNumber = await FrontEndHelper.FrontEndHelper.GetReverseProxyPortAsync(),
                    ServiceName = $"{tenantId}/{TenantApplicationTopicServiceName}/{topicName}/{subscriberName}/api"
                };

                HttpResponseMessage msg;
                PubSubMessage pubSubMsg = null;
                using (HttpClient httpClient = new HttpClient())
                {
                    string pubSubMsgJson = await httpClient.GetStringAsync(builder.Build());
                    if(!String.IsNullOrEmpty(pubSubMsgJson))
                        pubSubMsg = JsonConvert.DeserializeObject<PubSubMessage>(pubSubMsgJson);
                }

                HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                if (pubSubMsg != null)
                {
                    string msgJson = JsonConvert.SerializeObject(pubSubMsg.Message,
                        new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented
                        });
                    responseMessage.Content = new StringContent(msgJson);
                }
                else
                {
                    responseMessage.StatusCode = HttpStatusCode.NoContent;
                }

                return responseMessage;

                //using remoting
                //var topicSvc = ServiceProxy.Create<ISubscriberService>(new Uri($"fabric:/{tenantId}/{TenantApplicationTopicServiceName}/{topicName}/{subscriberName}"));
                //var msg = await topicSvc.Pop();
                //string msgJson = JsonConvert.SerializeObject(msg.Content,
                //    new JsonSerializerSettings
                //    {
                //        Formatting = Formatting.Indented
                //    });
                //responseMessage.Content = new StringContent(msgJson);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

        }

        private static async Task<int> GetReverseProxyPortAsync()
        {
            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();

            if (_reverseProxyPort == 0)
            {
                _reverseProxyPort = await portResolver.GetReverseProxyPortAsync();
            }

            return _reverseProxyPort;
        }
    }
}
