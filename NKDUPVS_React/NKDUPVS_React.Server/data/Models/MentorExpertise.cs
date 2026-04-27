using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class MentorExpertise
    {
        [Key]
        public int? Id { get; set; }
        public string? MentorCode { get; set; }
        public int? StudyProgram { get; set; }
        public int? Specialization { get; set; }
        public int? Priority { get; set; }
    }
}