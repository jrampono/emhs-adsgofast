using AdsGoFast.Models.Options;
using AdsGoFast.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AdsGoFast.Models
{
    public interface ILogAnalyticsContext {
        public IHttpClientFactory httpClient { get; set; }
        public string httpClientName { get; set; }
    }

    class LogAnalyticsContext : ILogAnalyticsContext
    {
        public IHttpClientFactory httpClient { get; set; }

        public string httpClientName { get; set; }
        public LogAnalyticsContext(IHttpClientFactory Client)
        {
            httpClient = Client;
            httpClientName = "LogAnalytics";
        }
    }
}
