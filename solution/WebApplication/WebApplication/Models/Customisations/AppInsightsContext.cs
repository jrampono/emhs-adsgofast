using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace WebApplication.Models
{
    public class AppInsightsContext 
    {
        Helpers.AzureSDK _azureSDK { get; set; }
        string _AppInsightsWorkspaceId { get; set; }

        public AppInsightsContext(Helpers.AzureSDK azureSDK, string AppInsightsWorkspaceId)
        {
            this._azureSDK = azureSDK;
            this._AppInsightsWorkspaceId = AppInsightsWorkspaceId;
        }

        public JArray ExecuteQuery(string Query)
        {;
            using var client = new HttpClient();           
            string token = _azureSDK.GetAzureRestApiToken("https://api.applicationinsights.io");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            JObject JsonContent = new JObject();
            JsonContent["query"] = Query;
            var postContent = new StringContent(JsonContent.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = client.PostAsync($"https://api.applicationinsights.io/v1/apps/{_AppInsightsWorkspaceId}/query", postContent).Result;

            JArray tables = new JArray();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Start to parse the response content
                HttpContent responseContent = response.Content;
                var content = response.Content.ReadAsStringAsync().Result;
                tables = ((JArray)(JObject.Parse(content)["tables"]));                
            }
            return tables;
        }
    }
}
