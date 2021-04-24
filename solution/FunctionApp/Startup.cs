/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/



using AdsGoFast;
using AdsGoFast.Models;
using AdsGoFast.Models.Options;
using AdsGoFast.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;


[assembly: FunctionsStartup(typeof(AdsGoFast.Startup))]

namespace AdsGoFast
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
   
            var config = new ConfigurationBuilder()
              .SetBasePath(Environment.CurrentDirectory)                           
              .AddUserSecrets("3956e7aa-4d13-430a-bb5f-a5f8f5a450ee"/*Assembly.GetExecutingAssembly()*/, true)
              .AddEnvironmentVariables()
              .Build();

            builder.Services.Configure<AuthOptions>(config.GetSection("AzureAdAuth"));
            builder.Services.Configure<ApplicationOptions>(config.GetSection("ApplicationOptions"));
            builder.Services.Configure<DownstreamAuthOptionsDirect>(config.GetSection("AzureAdAzureServicesDirect"));
            builder.Services.Configure<DownstreamAuthOptionsViaAppReg>(config.GetSection("AzureAdAzureServicesViaAppReg"));

            //builder.Services.AddSingleton<IConfiguration>(config); 
            builder.Services.AddSingleton<ISecurityAccessProvider>((provider) =>
            {
                var authOptions = provider.GetService<IOptions<DownstreamAuthOptionsViaAppReg>>();
                return new SecurityAccessProvider(authOptions);
            });

            //Inject Http Client for chained calling of core functions
            builder.Services.AddHttpClient("CoreFunctions",async (s,c) =>
            {
                var downstreamAuthOptionsViaAppReg = s.GetService<IOptions<DownstreamAuthOptionsViaAppReg>>();
                var appOptions = s.GetService<IOptions<ApplicationOptions>>();
                var authProvider = new AzureAuthenticationCredentialProvider(appOptions,downstreamAuthOptionsViaAppReg.Value);
                var token = authProvider.GetAzureRestApiToken(downstreamAuthOptionsViaAppReg.Value.Audience);
                c.DefaultRequestHeaders.Accept.Clear();
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));  //Set lifetime to five minutes

            //Inject Context for chained calling of core functions
            builder.Services.AddSingleton<ICoreFunctionsContext,CoreFunctionsContext>();

            //builder.Services.AddScoped<Logging>((s) =>
            //{
            //    return new Logging();
            //});


        }
    }
}

