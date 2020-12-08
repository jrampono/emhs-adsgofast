using Microsoft.AspNetCore.Http;
using System;
using Newtonsoft;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace WebApplication
{
    public class EasyAuthUserValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public EasyAuthUserValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.Request.Headers.ContainsKey("X-MS-CLIENT-PRINCIPAL-ID"))
            {
                var azureAppServicePrincipalIdHeader = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"][0];

                var uriString = $"{context.Request.Scheme}://{context.Request.Host}";
                var cookieContainer = new CookieContainer();
                var handler = new HttpClientHandler()
                {
                    CookieContainer = cookieContainer
                };

                foreach (var c in context.Request.Cookies)
                {
                    cookieContainer.Add(new Uri(uriString), new Cookie(c.Key, c.Value));
                }

                var jsonResult = string.Empty;
                using (var client = new HttpClient(handler))
                {
                    var res = await client.GetAsync($"{uriString}/.auth/me");
                    jsonResult = await res.Content.ReadAsStringAsync();
                }

                var obj = Newtonsoft.Json.Linq.JArray.Parse(jsonResult);

                var claims = new List<Claim>();
                foreach (var claim in obj[0]["user_claims"])
                {
                    claims.Add(new Claim(claim["typ"].ToString(), claim["val"].ToString()));
                }

                var identity = new GenericIdentity(azureAppServicePrincipalIdHeader);
                identity.AddClaims(claims);

                context.User = new GenericPrincipal(identity, null);
            }

            await _next(context);
        }
    }




        public static class EasyAuthUserValidationMiddlewareExtensions
        {
            public static IApplicationBuilder UseEasyAuthUserValidation(
                this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<EasyAuthUserValidationMiddleware>();
            }
        }
    

}