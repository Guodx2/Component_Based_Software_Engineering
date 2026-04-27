using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class SemesterPlanTask
    {
        [Key]
        public int? Id_SemesterPlanTask { get; set; }

        public int? TaskId { get; set; }

        public int? SemesterPlanId { get; set; }

        public string? CompletionFile { get; set; }
        public bool? IsRated { get; set; }
    }
}