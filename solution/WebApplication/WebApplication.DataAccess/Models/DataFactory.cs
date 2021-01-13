using System;

namespace WebApplication.Models
{
    public partial class DataFactory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ResourceGroup { get; set; }
        public Guid? SubscriptionUid { get; set; }
        public string DefaultKeyVaultUrl { get; set; }
        public Guid? LogAnalyticsWorkspaceId { get; set; }
    }
}
