using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NKDUPVS_React.Server.Models
{
    public class MentorRequest
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string? MenteeCode { get; set; }

        [Required]
        public string? MentorCode { get; set; }

        public DateTime? RequestDate { get; set; } = DateTime.Now;

        // Set default value to 1 (Laukiama patvirtinimo)
        [Required]
        public int? RequestStatusId { get; set; } = 1;

        public bool? IsRead { get; set; } = false;

        public string? RejectionReason { get; set; }

        // Navigation property
        public RequestStatus? RequestStatus { get; set; }
    }
}