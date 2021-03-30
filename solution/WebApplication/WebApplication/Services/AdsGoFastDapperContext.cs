using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using WebApplication.Models.Options;
using WebApplication.Services;

namespace WebApplication.Services
{
    public class AdsGoFastDapperContext 
    {
        private readonly AzureAuthenticationCredentialProvider _authProvider;
        private readonly string _connectionstring;

        public AdsGoFastDapperContext(AzureAuthenticationCredentialProvider authProvider, IOptions<ApplicationOptions> options)
        {
            var scsb = new SqlConnectionStringBuilder
            {
                DataSource = options.Value.AdsGoFastTaskMetaDataDatabaseServer,
                InitialCatalog = options.Value.AdsGoFastTaskMetaDataDatabaseName
            };
            _connectionstring = scsb.ConnectionString;
            _authProvider = authProvider;
        }

        public async Task<SqlConnection> GetConnection()
        {
            SqlConnection _con = new SqlConnection(_connectionstring);
            string _token = await _authProvider.GetMsalRestApiToken(new Azure.Core.TokenRequestContext(new string[] { "https://database.windows.net//.default" }), new System.Threading.CancellationToken());
            _con.AccessToken = _token;
            return _con;
        }
    }
}
