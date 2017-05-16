using System;

namespace RequestRouterService
{
    public class HttpServiceUriBuilder
    {
        public HttpServiceUriBuilder()
        {
            Scheme = "http";
            Host = "localhost";
            PortNumber = 19008;
        }

        public Uri Build()
        {
            UriBuilder uriBuilder = new UriBuilder(Scheme, Host, PortNumber)
            {
                Path = ServiceName
            };

            return uriBuilder.Uri;
        }

        public int PortNumber { get; }

        public string Scheme { get; }

        public string Host { get; }

        public string ServiceName { get; set; }

        public string ServicePathAndQuery { get; set; }
    }
}