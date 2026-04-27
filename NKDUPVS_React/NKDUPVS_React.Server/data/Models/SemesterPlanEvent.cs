using System.ComponentModel.DataAnnotations;

namespace NKDUPVS_React.Server.Models
{
    public class SemesterPlanEvent
    {
        [Key]
        public int? id_SemesterPlanEvent { get; set; }
        public int? Fk_SemesterPlanid_SemesterPlan { get; set; }
        public int? Fk_Eventid_Event { get; set; }
        public Event? Event { get; set; }
    }
}