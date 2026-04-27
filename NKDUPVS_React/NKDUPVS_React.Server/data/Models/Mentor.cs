using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class Mentor
    {
        [Key]
        public string? Code { get; set; } // Primary key

        public int? Department { get; set; }
        //public string PhoneNumber { get; set; } = string.Empty;

        public bool? AcceptingMentees { get; set; } = true;
    }
}