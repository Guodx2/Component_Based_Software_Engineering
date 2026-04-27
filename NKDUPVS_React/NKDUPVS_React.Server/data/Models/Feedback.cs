namespace NKDUPVS_React.Server.Models
{
    public class Feedback
    {
        public int? Id { get; set; }
        public int? SemesterPlanTaskId { get; set; }
        public int? Rating { get; set; } 
        public string? Comment { get; set; }
        public DateTime? SubmissionDate { get; set; } = DateTime.Now;
        public int? fk_Trainingid_Training { get; set; }
        public int? fk_Affairid_Affair { get; set; }
        public string? MenteeCode { get; set; }
    }
}