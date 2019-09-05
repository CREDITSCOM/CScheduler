using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CScheduler.Classes.Database
{
    [Table(name: "JobEvents")]
    public class JobEvent
    {
        [Key]
        public int ID { get; set; }
        public bool IsSuccessed { get; set; }
        public string Text { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime ResponseDate { get; set; }
        public SmartJob SmartJob { get; set; }
    }
}