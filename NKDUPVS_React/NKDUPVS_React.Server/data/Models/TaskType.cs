using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class TaskType
    {
        [Key]
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
}