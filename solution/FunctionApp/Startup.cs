/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/



using AdsGoFast;
using AdsGoFast.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System;
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

            //builder.Services.Configure<AuthOptions>(config.GetSection("AzureAd"));
            //builder.Services.AddSingleton<IConfiguration>(config); 
            builder.Services.AddSingleton<ISecurityAccessProvider,SecurityAccessProvider>();
            //builder.Services.AddScoped<Logging>((s) =>
            //{
            //    return new Logging();
            //});

           
        }
    }
}

