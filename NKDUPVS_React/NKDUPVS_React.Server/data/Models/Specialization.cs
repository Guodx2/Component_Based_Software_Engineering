using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class Specialization
    {
        [Key]
        public int? id { get; set; }
        public string? name { get; set; }
        // Optionally, link to the corresponding study program:
        public int? fk_id_StudyPrograms { get; set; }
    }
}