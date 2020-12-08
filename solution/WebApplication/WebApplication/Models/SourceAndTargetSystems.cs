using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models
{
    public partial class SourceAndTargetSystems
    {
        public long SystemId { get; set; }
        public string SystemName { get; set; }
        public string SystemType { get; set; }
        public string SystemDescription { get; set; }
        public string SystemServer { get; set; }
        public string SystemAuthType { get; set; }
        public string SystemUserName { get; set; }
        public string SystemSecretName { get; set; }
        public string SystemKeyVaultBaseUrl { get; set; }
        public string SystemJson { get; set; }
        public bool ActiveYn { get; set; }
    }
}
