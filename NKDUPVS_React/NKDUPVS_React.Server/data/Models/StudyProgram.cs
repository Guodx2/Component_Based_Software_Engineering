using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class StudyProgram
    {
        [Key]
        public int? id_StudyPrograms { get; set; }
        public string? name { get; set; }
    }
}