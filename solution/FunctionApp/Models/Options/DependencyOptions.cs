using System.Collections.Generic;

namespace AdsGoFast.Models.Options
{
    public class ApplicationOptions
    {
        public bool UseMSI { get; set; }
        public System.Int16 FrameworkWideMaxConcurrency { get; set; }
        public cServiceConnections ServiceConnections { get; set; }
        public cTimerTriggers TimerTriggers { get; set; }
        public cLocalPaths LocalPaths { get; set; }
        public cTestingOptions TestingOptions { get; set; }



        public class cServiceConnections
        {
            public string AppInsightsWorkspaceId { get; set; }
            public System.Int16 AppInsightsMaxNumberOfDaysToRequest { get; set; }
            public System.Int16 AppInsightsMinutesOverlap { get; set; } 
            public string CoreFunctionsURL { get; set; }
            public List<string> CoreFunctionsAllowedRoles { get; set; }
            public string AdsGoFastTaskMetaDataDatabaseServer { get; set; }
            public string AdsGoFastTaskMetaDataDatabaseName { get; set; }
            public bool AdsGoFastTaskMetaDataDatabaseUseTrustedConnection { get; set; }
        }

        public class cTimerTriggers { 
            public bool EnablePrepareFrameworkTasks { get; set; }
            public bool EnableRunFrameworkTasks { get; set; }
            public bool EnableGetADFStats { get; set; }
            public bool EnableGetActivityLevelLogs { get; set; }
        }

        public class cTestingOptions
        {
            public string TaskObjectTestFileLocation { get; set; }
            public bool GenerateTaskObjectTestFiles { get; set; }
            public string TaskMetaDataStorageAccount { get; set; }
            public string TaskMetaDataStorageContainer { get; set; }
            public string TaskMetaDataStorageFolder { get; set; }
        }

        public class cLocalPaths
        {
           public string SQLTemplateLocation { get; set; }
           public string KQLTemplateLocation { get; set; }
           public string HTMLTemplateLocation { get; set; }
        }


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