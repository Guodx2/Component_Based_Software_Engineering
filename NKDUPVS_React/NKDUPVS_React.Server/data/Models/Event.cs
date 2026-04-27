using System;

namespace NKDUPVS_React.Server.Models
{
    public class Event
    {
        public int? id_Event { get; set; }
        public string? name { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
        public string? address { get; set; }
        public string? comment { get; set; }
    }
}