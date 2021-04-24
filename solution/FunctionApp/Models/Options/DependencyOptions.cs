using System.Collections.Generic;

namespace AdsGoFast.Models.Options
{
    public class ApplicationOptions
    {
        public bool UseMSI { get; set; }
        
        public List<string> CoreFunctionsAllowedRoles { get; set; } 

           
    }

    public interface IAuthOptions
    {
        public string Audience { get; set; }
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string Tenant { get; set; }
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutCallbackPath { get; set; }
    }

    public class AuthOptions : IAuthOptions
    {
        public string Audience { get; set; }
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string Tenant { get; set; }
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutCallbackPath { get; set; }
    }

    public class DownstreamAuthOptionsDirect: IAuthOptions
    {
        public string Audience { get; set; }
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string Tenant { get; set; }
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutCallbackPath { get; set; }
    }

    public class DownstreamAuthOptionsViaAppReg: IAuthOptions
    {
        public string Audience { get; set; }
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string Tenant { get; set; }
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutCallbackPath { get; set; }
    }
}