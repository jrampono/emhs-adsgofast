namespace WebApplication.Models.Options
{
    public class ApplicationOptions
    {
        public bool UseMSI { get; set; }
        public string AdsGoFastTaskMetaDataDatabaseName { get; set; }
        public string AdsGoFastTaskMetaDataDatabaseServer { get; set; }
        public string AppInsightsWorkspaceId { get; set; }
    }
}