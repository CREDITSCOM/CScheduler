using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CScheduler.Classes.Database
{
    [Table(name: "Rules")]
    public class Rule
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Start date")]
        [RegularExpression(@"^([0-3][0-9]+)\/([0-3][0-9]+)\/([2-3][0-9][0-9][0-9]+) ([0-2][0-9]+)\:([0-5][0-9]+)\:([0-5][0-9]+)$", ErrorMessage = "Wrong value")]
        public string RegularDateFrom { get; set; }

        [Display(Name = "End date")]
        [RegularExpression(@"^([0-3][0-9]+)\/([0-3][0-9]+)\/([2-3][0-9][0-9][0-9]+) ([0-2][0-9]+)\:([0-5][0-9]+)\:([0-5][0-9]+)$", ErrorMessage = "Wrong value")]
        public string RegularDateTo { get; set; }

        [Display(Name = "Repeat every")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Wrong value")]
        public int RegularValue { get; set; }

        [Display(Name = "&nbsp;")]
        public PeriodEnum RegularPeriod { get; set; }

        [Display(Name = "Date & time")]
        [RegularExpression(@"^([0-3][0-9]+)\/([0-3][0-9]+)\/([2-3][0-9][0-9][0-9]+) ([0-2][0-9]+)\:([0-5][0-9]+)\:([0-5][0-9]+)$", ErrorMessage = "Wrong value")]
        public string OnceDate { get; set; }

        [Display(Name = "Cron expression")]
        public string CronExpression { get;set; }

        [Display(Name = "Presentation")]
        public string Presentation { get; set; }


        public static string GeneratePresentation(SmartJob sj)
        {
            string presentation = "";

            //Regular
            if (sj.ExecutionMode == SmartJob.ExecutionModeEnum.Regular)
            {
                presentation = string.Format("{0} - {1}",
                     string.IsNullOrEmpty(sj.Rule.RegularDateFrom) ? "..." : sj.Rule.RegularDateFrom,
                     string.IsNullOrEmpty(sj.Rule.RegularDateTo) ? "..." : sj.Rule.RegularDateTo);

                if (sj.Rule.RegularValue == 0)
                {
                    presentation = string.Format("{0} No repeat", presentation);
                }
                else
                {
                    presentation = string.Format("{0} Every {1} {2}", presentation, sj.Rule.RegularValue, sj.Rule.RegularPeriod.ToString());
                }
            }
            //Once
            else if (sj.ExecutionMode == SmartJob.ExecutionModeEnum.Once)
            {
                presentation = string.Format("Once: {0}", sj.Rule.OnceDate);
            }
            //CronExpression
            else if (sj.ExecutionMode == SmartJob.ExecutionModeEnum.CronExpression)
            {
                presentation = string.Format("Cron expression: {0}", sj.Rule.CronExpression);
            }

            return presentation;
        }
    }

    public enum PeriodEnum
    {
        [Display(Name = "Seconds")]
        Second = 1,

        [Display(Name = "Minutes")]
        Minute = 2,

        [Display(Name = "Hours")]
        Hour = 3,

        [Display(Name = "Days")]
        Day = 4
    }
}