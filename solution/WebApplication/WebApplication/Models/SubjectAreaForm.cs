using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public partial class SubjectAreaForm
    {
        public int SubjectAreaFormId { get; set; }
        public string FormJson { get; set; }
        public byte FormStatus { get; set; }
        public string UpdatedBy { get; set; }

    }
}
