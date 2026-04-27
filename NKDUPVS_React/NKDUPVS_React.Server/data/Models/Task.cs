using System;

namespace NKDUPVS_React.Server.Models
{
    public class Task
    {
        public int? Id_Task { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? MaterialLink { get; set; }
        public DateTime? Deadline { get; set; }
        //public string? CompletionFile { get; set; }
        public bool? IsAssigned { get; set; }
        //public bool IsRated { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public int? TaskTypeId { get; set; }
        public TaskType? TaskType { get; set; }
    }
}