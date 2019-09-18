using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CScheduler.Classes.Database
{
    [Table(name: "SmartJobs")]
    public class SmartJob : IJob
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Method")]
        [Required]
        public string Method { get; set; }

        [Display(Name = "Address")]
        [Required]
        public string Address { get; set; }

        [Display(Name = "Network")]
        public CreditsNet CreditsNet { get; set; }

        [Display(Name = "Created at")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Author")]
        public ApplicationUser CreatedBy { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Rule")]
        public Rule Rule { get; set; }

        [Display(Name = "Execution mode")]
        public ExecutionModeEnum ExecutionMode { get; set; }

        [Display(Name = "Errors")]
        public int Errors { get; set; }

        [Display(Name = "Executes")]
        public int Executes { get; set; }

        [Display(Name = "Events")]
        public List<JobEvent> Events { get; set; }

        [Display(Name = "Delete task after execution")]
        public bool DeleteTaskAfterExecution { get; set; }

        public bool IsDeleted { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> CreditsNetList
        {
            get
            {
                var list = new List<SelectListItem>();
                using (var dbContext = new DatabaseContext())
                {
                    foreach (var net in dbContext.CreditsNets)
                    {
                        list.Add(new SelectListItem { Text = net.Name, Value = net.ID.ToString() });
                    }                    
                }

                return list;
            }
        }

        [NotMapped]
        public IEnumerable<SelectListItem> RegularPeriodList
        {
            get
            {
                var list = new List<SelectListItem>();
                foreach (var item in Enum.GetValues(typeof(PeriodEnum)).Cast<PeriodEnum>())
                {
                    if (item == PeriodEnum.Second)
                        continue;

                    list.Add(new SelectListItem { Text = item.ToString(), Value = ((int)item).ToString() });
                }

                return list;
            }
        }


        public SmartJob()
        {
            
        }

        public async Task Execute(IJobExecutionContext Context)
        {
            await QuartzTasks.ExecuteJob(Context);
        }


        public enum ExecutionModeEnum : int
        {
            [Display(Name = "Regular")]
            Regular = 1,

            [Display(Name = "Once")]
            Once = 2,

            [Display(Name = "Cron expression")]
            CronExpression = 3
        }
    }
}