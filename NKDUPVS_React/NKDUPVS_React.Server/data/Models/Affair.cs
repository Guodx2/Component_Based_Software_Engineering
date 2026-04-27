namespace NKDUPVS_React.Server.Models
{
    public class Affair
    {
        public int? id_Affair { get; set; }
        public int? event_id { get; set; }
        public Event? Event { get; set; }
    }
}