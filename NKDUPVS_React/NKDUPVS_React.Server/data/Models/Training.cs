using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace NKDUPVS_React.Server.Models
{
    public class Training
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? id_Training { get; set; }
        public string? fk_ViceDeadForStudiescode { get; set; }
        
        // Reference to the associated event
        public int? event_id { get; set; }
        public Event? Event { get; set; }
    }
}