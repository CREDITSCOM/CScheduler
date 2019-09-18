using Credtis_Api_Connect;
using Credtis_Api_Connect.Model;
using CScheduler.Classes.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Threading;
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

        public async Task<JsonResult> AddNewTask(NewTaskModel model)
        {
            var result = new ApiJsonResult();

            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var user = dbContext.Users.FirstOrDefault(x => x.ApiKey == model.ApiKey);
                    if (user == null)
                    {
                        result.IsSuccess = false;
                        result.Message = "The key value is wrong";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(model.Name))
                        {
                            result.IsSuccess = false;
                            result.Message = "The name value is required";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(model.Network))
                            {
                                result.IsSuccess = false;
                                result.Message = "The network value is required";
                            }
                            else
                            {
                                if (model.Network != "testnet-r4_2" && model.Network != "DevsDappsTestnet" && model.Network != "CreditsNetwork")
                                {
                                    result.IsSuccess = false;
                                    result.Message = "The network value is wrong. Supported values: 'testnet-r4_2', 'DevsDappsTestnet', 'CreditsNetwork'";
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(model.Method))
                                    {
                                        result.IsSuccess = false;
                                        result.Message = "The method value is required";
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(model.Address))
                                        {
                                            result.IsSuccess = false;
                                            result.Message = "The address value is required";
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(model.ExecutionMode))
                                            {
                                                result.IsSuccess = false;
                                                result.Message = "The executionMode value is required";
                                            }
                                            else
                                            {
                                                if (model.ExecutionMode == "Regular")
                                                {
                                                    if (CheckDate(model.RegularDateFrom) == DateTime.MinValue)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The regularDateFrom value is wrong. Supported mask: mm-DD-YYYY-hh-MM-ss";
                                                    }
                                                    else if (CheckDate(model.RegularDateTo) == DateTime.MinValue)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The regularDateTo value is wrong. Supported mask: mm-DD-YYYY-hh-MM-ss";
                                                    }
                                                    else if (CheckPeriodName(model.RegularPeriod) == 0)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The regularPeriod value is wrong. Supported values: 'Minutes', 'Hours', 'Days'";
                                                    }
                                                    else if (CheckPeriodValue(model.RegularValue) == 0)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The regularValue value is wrong. Supported values: 1, 2, 3, and so on...";
                                                    }
                                                }
                                                else if (model.ExecutionMode == "Once")
                                                {
                                                    if (CheckDate(model.OnceDate) == DateTime.MinValue)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The onceDate value is wrong. Supported mask: mm-DD-YYYY-hh-MM-ss";
                                                    }
                                                }
                                                else if (model.ExecutionMode == "InSeconds")
                                                {
                                                    if (CheckInSeconds(model.InSecondsValue) == false)
                                                    {
                                                        result.IsSuccess = false;
                                                        result.Message = "The InSecondsValue value is wrong. Value must be integer value";
                                                    }
                                                    else
                                                    {
                                                        var seconds = Convert.ToInt32(model.InSecondsValue);
                                                        var dateExecution = DateTime.Now.AddSeconds(seconds);
                                                        model.OnceDate = dateExecution.ToString("MM/dd/yyyy HH:mm:ss");
                                                    }
                                                }
                                                else if (model.ExecutionMode == "CronExpression")
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
                                                    smartJob.Name = model.Name;
                                                    smartJob.IsActive = true;
                                                    smartJob.Method = model.Method;
                                                    smartJob.Address = model.Address;
                                                    smartJob.IsDeleted = false;
                                                    smartJob.DeleteTaskAfterExecution = model.DeleteTaskAfterExecution == "1";
                                                    smartJob.CreditsNet = dbContext.CreditsNets.FirstOrDefault(x => x.Name == model.Network);
                                                    smartJob.ExecutionMode = CheckExecutionMode(model.ExecutionMode);
                                                    smartJob.Rule.RegularDateFrom = CheckDate(model.RegularDateFrom).ToString("MM/dd/yyyy HH:mm:ss");
                                                    smartJob.Rule.RegularDateTo = CheckDate(model.RegularDateTo).ToString("MM/dd/yyyy HH:mm:ss");
                                                    smartJob.Rule.RegularPeriod = ConvertPeriod(model.RegularPeriod);
                                                    smartJob.Rule.RegularValue = CheckPeriodValue(model.RegularValue);
                                                    smartJob.Rule.OnceDate = CheckDate(model.OnceDate).ToString("MM/dd/yyyy HH:mm:ss");
                                                    smartJob.Rule.CronExpression = model.CronExpression;
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
            catch (DbEntityValidationException err)
            {
                var outputLines = new List<string>();

                foreach (var eve in err.EntityValidationErrors)
                {
                    outputLines.Add(
                        $"{DateTime.Now}: Entity of type \"{eve.Entry.Entity.GetType().Name}\" in state \"{eve.Entry.State}\" has the following validation errors:");
                    outputLines.AddRange(eve.ValidationErrors.Select(ve =>
                        $"- Property: \"{ve.PropertyName}\", Error: \"{ve.ErrorMessage}\""));
                }

                result.IsSuccess = false;
                result.Message = String.Join(", ", outputLines.ToArray());
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

        private string GetIP(string Network)
        {
            if (Network == "CreditsNetwork")
                return "161.156.96.26";

            else if (Network == "testnet-r4_2")
                return "89.111.33.169";

            else if (Network == "DevsDappsTestnet")
                return "161.156.96.22";

            else
                return null;
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult DeploySmartContract(DeployModel model)
        {
            var result = new ApiJsonResult();

            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var networkIP = GetIP(model.Network);

                    if (!string.IsNullOrEmpty(networkIP))
                    {
                        if (string.IsNullOrEmpty(model.PublicKey) || string.IsNullOrEmpty(model.PrivateKey))
                        {
                            var email = User?.Identity?.Name;
                            var user = dbContext.Users.FirstOrDefault(x => x.Email == email);
                            model.PublicKey = user?.PublicKey;
                            model.PrivateKey = user?.PrivateKey;
                        }

                        using (Work work = new Work(networkIP))
                        {
                            var transaction = work.Api.SendTransaction<CreateTransactionModel>(new CreateTransactionModel(new TransactionCreateModel
                            {
                                Amount = "0",
                                Fee = "1",
                                Source = model.PublicKey,
                                Smart = new SmartCreateModel { Code = model.JavaCode }
                            }), Base58Check.Base58CheckEncoding.DecodePlain(model.PrivateKey));

                            var source = Base58Check.Base58CheckEncoding.EncodePlain(transaction.Source);
                            var target = Base58Check.Base58CheckEncoding.EncodePlain(transaction.Target);

                            result.IsSuccess = true;
                            result.Address = target;
                            result.Message = "Ok!";
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Network is wrong.";
                    }                        
                }
            }
            catch (Exception err)
            {
                result.IsSuccess = false;
                result.Message = "Error: " + err.ToString();
            }

            return new JsonResult
            {
                MaxJsonLength = Int32.MaxValue,
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult DeploySmartContracts(DeployModel model)
        {
            var result = new ApiJsonResult();

            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var networkIP = GetIP(model.Network);

                    if (!string.IsNullOrEmpty(networkIP))
                    {
                        if (string.IsNullOrEmpty(model.PublicKey) || string.IsNullOrEmpty(model.PrivateKey))
                        {
                            var email = User?.Identity?.Name;
                            var user = dbContext.Users.FirstOrDefault(x => x.Email == email);
                            model.PublicKey = user?.PublicKey;
                            model.PrivateKey = user?.PrivateKey;
                        }

                        using (Work work = new Work(networkIP))
                        {
                            var transaction = work.Api.SendTransaction<CreateTransactionModel>(new CreateTransactionModel(new TransactionCreateModel
                            {
                                Amount = "0",
                                Fee = "1",
                                Source = model.PublicKey,
                                Smart = new SmartCreateModel { Code = model.JavaCode }
                            }), Base58Check.Base58CheckEncoding.DecodePlain(model.PrivateKey));

                            var source = Base58Check.Base58CheckEncoding.EncodePlain(transaction.Source);
                            var target = Base58Check.Base58CheckEncoding.EncodePlain(transaction.Target);

                            result.IsSuccess = true;
                            result.Address = target;
                            result.Message = "Ok!";
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Network is wrong.";
                    }
                }
            }
            catch (Exception err)
            {
                result.IsSuccess = false;
                result.Message = "Error: " + err.ToString();
            }

            return new JsonResult
            {
                MaxJsonLength = Int32.MaxValue,
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private DateTime CheckDate(string date)
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

        private bool CheckInSeconds(string value)
        {
            int ignoreMe;
            return int.TryParse(value, out ignoreMe);
        }

        private int CheckPeriodName(string periodName)
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

        private int CheckPeriodValue(string periodValue)
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

        private ExecutionModeEnum CheckExecutionMode(string executionMode)
        {
            if (executionMode == "Regular")
                return ExecutionModeEnum.Regular;
            else if (executionMode == "Once")
                return ExecutionModeEnum.Once;
            else
                return ExecutionModeEnum.CronExpression;
        }

        private PeriodEnum ConvertPeriod(string executionMode)
        {
            if (executionMode == "Days")
                return PeriodEnum.Day;
            else if (executionMode == "Hours")
                return PeriodEnum.Hour;
            else
                return PeriodEnum.Minute;
        }

        private class CryptoPollJsonResult
        {
            public int roundFinishedAt { get; set; }
        }

        private class ApiJsonResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public object Address { get; set; }
        }

        public class DeployModel
        {
            public string JavaCode { get; set; }
            public string Network { get; set; }
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }

        public class NewTaskModel
        {
            public string ApiKey { get; set; }
            public string Name { get; set; }
            public string Network { get; set; }
            public string Method { get; set; }
            public string Address { get; set; }
            public string ExecutionMode { get; set; }
            public string RegularDateFrom { get; set; }
            public string RegularDateTo { get; set; }
            public string RegularPeriod { get; set; }
            public string RegularValue { get; set; }
            public string OnceDate { get; set; }
            public string CronExpression { get; set; }
            public string InSecondsValue { get; set; }
            public string DeleteTaskAfterExecution { get; set; }
        }
    }
}