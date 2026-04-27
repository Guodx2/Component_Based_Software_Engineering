using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class RejectUserRequest
    {
        [Required]
        public string? Reason { get; set; }
    }
}