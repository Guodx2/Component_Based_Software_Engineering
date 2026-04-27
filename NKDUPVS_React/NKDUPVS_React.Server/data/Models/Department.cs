using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class Department
    {
        [Key]
        public int? id_Departments { get; set; }
        public string? name { get; set; }
    }
}