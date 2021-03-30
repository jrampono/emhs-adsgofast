using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace WebApplication.Models
{
    public partial class SubjectArea
    {
        public int SubjectAreaId { get; set; }
        public string SubjectAreaName { get; set; }
        public bool ActiveYn { get; set; }
        public int? SubjectAreaFormId { get; set; }
        public string DefaultTargetSchema { get; set; }
        public string UpdatedBy { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ValidFrom { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ValidTo { get; set; }
    }
}
