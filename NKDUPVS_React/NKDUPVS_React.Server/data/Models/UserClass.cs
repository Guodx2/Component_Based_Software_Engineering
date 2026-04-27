using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NKDUPVS_React.Server.Models
{
    public class UserClass
    {
        public int? Id { get; set; }
        public string? UserCode { get; set; }
        public string? ClassCode { get; set; }

        public int? Department { get; set; }
        public string? Auditorium { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Teacher { get; set; } = string.Empty; 
        public int? Duration { get; set; }
        public int? Type { get; set; }


        public User? User { get; set; }
        public Class? Class { get; set; }
    }
}