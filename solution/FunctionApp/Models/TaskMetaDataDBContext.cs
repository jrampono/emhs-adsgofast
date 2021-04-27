using AdsGoFast.Models.Options;
using AdsGoFast.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AdsGoFast.Models
{
    public interface ITaskMetaDataDBContext
    {
        public IHttpClientFactory httpClient { get; set; }
        public string httpClientName { get; set; }
    }
  

    class TaskMetaDataDBContext : ITaskMetaDataDBContext
    {
        public IHttpClientFactory httpClient { get; set; }

        public string httpClientName { get; set; }
        public TaskMetaDataDBContext(IHttpClientFactory Client)
        {
            httpClient = Client;
            httpClientName = "TaskMetaDataDatabase";
        }
    }
}
