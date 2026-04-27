using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class Mentee
    {
        [Key]
        public string? Code { get; set; } 

        public int? StudyProgram { get; set; }

        public int? Specialization { get; set; }

        public string? MentorCode { get; set; }
    }
}