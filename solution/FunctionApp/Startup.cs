/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
              .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
              .AddEnvironmentVariables()
              .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.AddScoped<Logging>((s) =>
            {
                return new Logging();
            });
        }
    }
}
