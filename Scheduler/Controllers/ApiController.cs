using CScheduler.Classes.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CScheduler.Controllers
{
    public class ApiController : Controller
    {
        //Возвращает время до окончания раунда
        public JsonResult CryptoPollGetFinishTime(string CurrentNet)
        {
            var result = new CryptoPollJsonResult() { roundFinishedAt = 600 };

            using (var dbContext = new DatabaseContext())
            {
                var creditsNetID = dbContext.CreditsNets.FirstOrDefault(x => x.Name == "DevsDappsTestnet")?.ID;

                if (creditsNetID != null)
                {
                    var smartJob = dbContext.SmartJobs
                        .Include("CreditsNet")
                        .Include("Rule").FirstOrDefault(x =>
                            x.CreditsNet.ID == creditsNetID
                            && x.IsActive == true
                            && x.Method == "executeRound"
                        );

                    if (smartJob != null && smartJob.ExecutionMode == SmartJob.ExecutionModeEnum.Regular && smartJob.Rule.RegularValue > 0)
                    {
                        int intervalSeconds = smartJob.Rule.RegularValue;

                        if (smartJob.Rule.RegularPeriod == PeriodEnum.Minute)
                            intervalSeconds *= 60;
                        else if (smartJob.Rule.RegularPeriod == PeriodEnum.Hour)
                            intervalSeconds *= (60 * 60);
                        else if (smartJob.Rule.RegularPeriod == PeriodEnum.Day)
                            intervalSeconds *= (60 * 60 * 24);

                        var dateFrom = DateTime.ParseExact(smartJob.Rule.RegularDateFrom, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        var currDateTime = DateTime.Now;
                        while (dateFrom < currDateTime)
                        {
                            dateFrom = dateFrom.AddSeconds(intervalSeconds);
                        }

                        result.roundFinishedAt = (int)(dateFrom - currDateTime).TotalMilliseconds + 10000;
                    }
                }
            }

            return new JsonResult
            {
                MaxJsonLength = Int32.MaxValue,
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public class CryptoPollJsonResult
        {
            public int roundFinishedAt { get; set; }
        }
    }
}