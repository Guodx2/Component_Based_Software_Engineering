using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class SemesterPlan
    {
        [Key]
        public int? Id_SemesterPlan { get; set; }
        public string? MenteeCode { get; set; }
        public DateTime? SemesterStartDate { get; set; }
        public DateTime? SemesterEndDate { get; set; }
        public string? MentorCode { get; set; }
        public ICollection<SemesterPlanEvent>? SemesterPlanEvents { get; set; }
    }
}