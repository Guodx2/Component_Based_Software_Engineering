using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NKDUPVS_React.Server.Models
{
    public class RejectionHistory
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        [StringLength(8)]
        public string? UserCode { get; set; }

        [Required]
        [StringLength(255)]
        public string? Reason { get; set; }
        
        public DateTime? RejectedAt { get; set; } = DateTime.UtcNow;
    }
}