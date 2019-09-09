using CScheduler.Classes.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static CScheduler.Classes.Database.SmartJob;

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

        public async Task<JsonResult> AddNewTask(string apiKey = "", string name = "", string network = "", string method = "", string address = "", string executionMode = "", string regularDateFrom = "", string regularDateTo = "", string regularPeriod = "", string regularValue = "", string onceDate = "", string cronExpression = "")
        {
            var result = new ApiJsonResult();

            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var user = dbContext.Users.FirstOrDefault(x => x.ApiKey == apiKey);
                    if (user == null)
                    {
                        result.IsSuccess = false;
                        result.Message = "The key value is wrong";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(name))
                        {
                            result.IsSuccess = false;
                            result.Message = "The name value is required";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(network))
                            {
                                result.IsSuccess = false;
                                result.Message = "The network value is required";
                            }
                            else
                            {
                                if (network != "testnet-r4_2" && network != "DevsDappsTestnet" && network != "CreditsNetwork")
                                {
                                    result.IsSuccess = false;
                                    result.Message = "The network value is wrong. Supported values: 'testnet-r4_2', 'DevsDappsTestnet', 'CreditsNetwork'";
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(method))
                                    {
                                        result.IsSuccess = false;
                                        result.Message = "The method value is required";
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(address))
                                        {
                                            result.IsSuccess = false;
                                            result.Message = "The address value is required";
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(executionMode))
                                            {
                                                result.IsSuccess = false;
                                                result.Message = "The executionMode value is required";
                                            }
                                            else
                                            {
                                                if (executionMode == "Regular")
                                                {
                                                    if (checkDate(regularDateFrom) == DateTime.MinValue)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The regularDateFrom value is wrong. Supported mask: mm-DD-YYYY-hh-MM-ss";
                                                    }
                                                    else if (checkDate(regularDateTo) == DateTime.MinValue)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The regularDateTo value is wrong. Supported mask: mm-DD-YYYY-hh-MM-ss";
                                                    }
                                                    else if (checkPeriodName(regularPeriod) == 0)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The regularPeriod value is wrong. Supported values: 'Minutes', 'Hours', 'Days'";
                                                    }
                                                    else if (checkPeriodValue(regularValue) == 0)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The regularValue value is wrong. Supported values: 1, 2, 3, and so on...";
                                                    }
                                                }
                                                else if (executionMode == "Once")
                                                {
                                                    if (checkDate(onceDate) == DateTime.MinValue)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The onceDate value is wrong. Supported mask: mm-DD-YYYY-hh-MM-ss";
                                                    }
                                                }
                                                else if (executionMode == "CronExpression")
                                                {

                                                }
                                                else
                                                {
                                                    result.IsSuccess = false;
                                                    result.Message = "The executionMode value is wrong. Supported values: 'Regular', 'Once', 'CronExpression'";
                                                }

                                                //Ошибок нет, создаем новую задачу
                                                if (!result.IsSuccess)
                                                {
                                                    var smartJob = new SmartJob();
                                                    smartJob = new SmartJob();
                                                    smartJob.CreatedBy = user;
                                                    smartJob.CreatedAt = DateTime.Now;
                                                    smartJob.Rule = new Rule();
                                                    smartJob.Events = new List<JobEvent>();
                                                    smartJob.Name = name;
                                                    smartJob.IsActive = true;
                                                    smartJob.Method = method;
                                                    smartJob.Address = address;
                                                    smartJob.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == network);
                                                    smartJob.ExecutionMode = checkExecutionMode(executionMode);
                                                    smartJob.Rule.RegularDateFrom = checkDate(regularDateFrom).ToString("MM/dd/yyyy HH:mm:ss");
                                                    smartJob.Rule.RegularDateTo = checkDate(regularDateTo).ToString("MM/dd/yyyy HH:mm:ss");
                                                    smartJob.Rule.RegularPeriod = convertPeriod(regularPeriod);
                                                    smartJob.Rule.RegularValue = checkPeriodValue(regularValue);
                                                    smartJob.Rule.OnceDate = checkDate(onceDate).ToString("MM/dd/yyyy HH:mm:ss");
                                                    smartJob.Rule.CronExpression = cronExpression;
                                                    smartJob.Rule.Presentation = Rule.GeneratePresentation(smartJob);

                                                    dbContext.SmartJobs.Add(smartJob);
                                                    await dbContext.SaveChangesAsync();

                                                    //Обновляем данные по задаче
                                                    await QuartzTasks.UpdateJob(smartJob.ID);

                                                    result.IsSuccess = true;
                                                    result.Message = "Action completed";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                result.IsSuccess = false;
                result.Message = err.ToString();
            }

            return new JsonResult
            {
                MaxJsonLength = Int32.MaxValue,
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public DateTime checkDate(string date)
        {
            try
            {
                var arr = date.Split('-');
                var month = Convert.ToInt32(arr[0]);
                var day = Convert.ToInt32(arr[1]);
                var year = Convert.ToInt32(arr[2]);
                var hour = Convert.ToInt32(arr[3]);
                var min = Convert.ToInt32(arr[4]);
                var sec = Convert.ToInt32(arr[5]);

                return new DateTime(year, month, day, hour, min, sec);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public int checkPeriodName(string periodName)
        {
            if (periodName == "Minutes")
                return 2;
            else if (periodName == "Hours")
                return 3;
            else if (periodName == "Days")
                return 4;
            else
                return 0;
        }

        public int checkPeriodValue(string periodValue)
        {
            try
            {
                return Convert.ToInt32(periodValue);
            }
            catch
            {
                return 0;
            }
        }

        public ExecutionModeEnum checkExecutionMode(string executionMode)
        {
            if (executionMode == "Regular")
                return ExecutionModeEnum.Regular;
            else if (executionMode == "Once")
                return ExecutionModeEnum.Once;
            else
                return ExecutionModeEnum.CronExpression;
        }

        public PeriodEnum convertPeriod(string executionMode)
        {
            if (executionMode == "Days")
                return PeriodEnum.Day;
            else if (executionMode == "Hours")
                return PeriodEnum.Hour;
            else
                return PeriodEnum.Minute;
        }

        public class CryptoPollJsonResult
        {
            public int roundFinishedAt { get; set; }
        }

        public class ApiJsonResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }
    }
}