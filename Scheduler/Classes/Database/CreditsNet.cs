using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CScheduler.Classes.Database
{
    [Table(name: "CreditsNets")]
    public class CreditsNet
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "End point")]
        public string EndPoint { get; set; }
    }
}