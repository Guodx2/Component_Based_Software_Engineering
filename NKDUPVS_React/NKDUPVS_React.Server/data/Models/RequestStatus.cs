using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class RequestStatus
    {
        [Key]
        public int? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
    }
}