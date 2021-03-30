using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public partial class ExecutionEngine
    {
        public long EngineId { get; set; }
        public string EngineName { get; set; }
        public string ResouceName { get; set; }
        public string ResourceGroup { get; set; }
        public Guid? SubscriptionUid { get; set; }
        public string DefaultKeyVaultUrl { get; set; }
        public Guid? LogAnalyticsWorkspaceId { get; set; }
        public string EngineJson { get; set; }
    }
}
