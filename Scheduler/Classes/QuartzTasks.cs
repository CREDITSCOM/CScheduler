using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CScheduler.Classes.Database
{
    public class QuartzTasks
    {
        public static IScheduler _scheduler;

        //Вызывается 1 раз при начале работы сервера
        public async static void Initialise()
        {
            if (Environment.MachineName == "ANDY0451")
                return;

            NameValueCollection props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);

            _scheduler = await factory.GetScheduler();
            await _scheduler.Start();

            using (var dbContext = new DatabaseContext())
            {
                foreach (var smartJob in await dbContext.SmartJobs.Include("Rule").Where(x => x.IsActive).ToListAsync())
                {
                    await InitJob(smartJob);
                }
            }
        }

        //Инициализация задачи. Вызывается при старте сервера или после обновления задачи
        public async static Task InitJob(SmartJob smartJob, bool AllowLog = true)
        {
            try
            {
                if (smartJob.IsActive == false)
                    return;

                //Создаем задачу
                IJobDetail job = JobBuilder.Create<SmartJob>().WithIdentity(smartJob.ID.ToString()).Build();

                //Создаем триггер для задачи
                ITrigger trigger;

                //Режим: Регулярное выполнение
                if (smartJob.ExecutionMode == SmartJob.ExecutionModeEnum.Regular)
                {
                    DateTime startAt = string.IsNullOrEmpty(smartJob.Rule.RegularDateFrom) ? DateTime.MinValue : DateTime.ParseExact(smartJob.Rule.RegularDateFrom, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime endAt = string.IsNullOrEmpty(smartJob.Rule.RegularDateTo) ? DateTime.MaxValue : DateTime.ParseExact(smartJob.Rule.RegularDateTo, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    var interval = 0;
                    if (smartJob.Rule.RegularValue > 0)
                    {
                        if (smartJob.Rule.RegularPeriod == PeriodEnum.Second) interval = smartJob.Rule.RegularValue;
                        else if (smartJob.Rule.RegularPeriod == PeriodEnum.Minute) interval = smartJob.Rule.RegularValue * 60;
                        else if (smartJob.Rule.RegularPeriod == PeriodEnum.Hour) interval = smartJob.Rule.RegularValue * 60 * 60;
                        else if (smartJob.Rule.RegularPeriod == PeriodEnum.Day) interval = smartJob.Rule.RegularValue * 60 * 60 * 24;
                    }

                    if (interval == 0)
                    {
                        trigger = TriggerBuilder.Create().StartAt(startAt).EndAt(endAt).Build();
                    }
                    else
                    {
                        trigger = TriggerBuilder.Create().StartAt(startAt).EndAt(endAt).WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever()).Build();
                    }
                }

                //Режим: Одноразовое выполнение
                else if (smartJob.ExecutionMode == SmartJob.ExecutionModeEnum.Once)
                {
                    DateTime startAt = string.IsNullOrEmpty(smartJob.Rule.OnceDate) ? DateTime.MinValue : DateTime.ParseExact(smartJob.Rule.OnceDate, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    trigger = TriggerBuilder.Create().StartAt(startAt).Build();
                }

                //Режим: Cron выражение
                else
                {
                    trigger = TriggerBuilder.Create().WithCronSchedule(smartJob.Rule.CronExpression).Build();
                }

                //Добавляем в планировщик задачу и триггер
                await _scheduler.ScheduleJob(job, trigger);

                //Пишем лог
                if (AllowLog)
                {
                    await AddEvent(smartJob.ID, "Initialized");
                }
            }
            catch (Exception err)
            {
                //Пишем лог
                await AddEvent(smartJob.ID, "StartJob error: " + err.Message + " Smart: " + JsonConvert.SerializeObject(smartJob));
            }
        }

        //Вызывается из SmartJob.cs при наступлении очередного времени выполнения
        public async static Task ExecuteJob(IJobExecutionContext JobContext)
        {
            if (Environment.MachineName == "ANDY0451")
                return;

            int smartJobID = 0;

            try
            {
                smartJobID = Convert.ToInt32(JobContext.JobDetail.Key.Name);

                await ExecuteJob(smartJobID);
            }
            catch (Exception err)
            {
                //Пишем лог
                await AddEvent(smartJobID, "ExecuteJob error: " + err.Message);
            }
        }

        //Вызывается из ExecuteJob при наступлении очередного времени выполнения задания или по кнопке "Выполнить" из контроллера Home
        public async static Task ExecuteJob(int SmartJobID)
        {
            if (Environment.MachineName == "ANDY0451")
                return;

            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var smartJob = await dbContext.SmartJobs.Include("CreditsNet").Include("CreatedBy").FirstOrDefaultAsync(x => x.ID == SmartJobID);

                    if (smartJob != null)
                    {
                        ServicePointManager.Expect100Continue = false;

                        var jsonData = new JsonData();
                        jsonData.Amount = "0";
                        jsonData.Smart.Method = smartJob.Method;
                        jsonData.Source = smartJob.CreatedBy.PublicKey;
                        jsonData.Target = smartJob.Address;
                        jsonData.Priv = smartJob.CreatedBy.PrivateKey;

                        string dataPost = JsonConvert.SerializeObject(jsonData);

                        var data = Encoding.ASCII.GetBytes(dataPost);
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(smartJob.CreditsNet.EndPoint);
                        req.Timeout = 3600000;
                        req.CookieContainer = new CookieContainer();
                        req.Proxy = null;
                        req.ContentType = "application/json";
                        req.Accept = "*/*";
                        req.Method = "POST";
                        req.ContentLength = data.Length;

                        using (var stream = req.GetRequestStream())
                        {
                            stream.Write(data, 0, data.Length);
                        }

                        var response = (HttpWebResponse)req.GetResponse();
                        var textResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();

                        //Debug.WriteLine("smartJob.ID " + smartJobID.ToString());

                        //Пишем лог
                        await AddEvent(SmartJobID, "Task completed: " + textResponse);

                        //Помечаем на удаление, если установлен флаг
                        if (smartJob.DeleteTaskAfterExecution == true)
                        {
                            smartJob.IsDeleted = true;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                //Пишем лог
                await AddEvent(SmartJobID, "ExecuteJob error: " + err.Message);
            }
        }

        //Вызывается после сохранения SmartJob
        public async static Task UpdateJob(int SmartJobID)
        {
            int smartJobID = 0;

            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var smartJob = dbContext.SmartJobs.Include("Rule").FirstOrDefault(x => x.ID == SmartJobID);

                    if (smartJob != null)
                    {
                        smartJobID = smartJob.ID;

                        //Удаляем задачу
                        await DeleteJob(smartJobID, false);

                        //Запускаем заново. Если задача не активна, то она не будет запущена
                        await InitJob(smartJob, false);

                        //Пишем лог
                        await AddEvent(smartJobID, "Task updated");
                    }
                }
            }
            catch (Exception err)
            {
                //Пишем лог
                await AddEvent(smartJobID, "UpdateJob error: " + err.Message);
            }
        }

        //Вызывается после удаления SmartJob
        public static async Task DeleteJob(int SmartJobID, bool AllowLog = true)
        {
            try
            {
                using (var dbContext = new DatabaseContext())
                {
                    var smartJob = dbContext.SmartJobs.FirstOrDefault(x => x.ID == SmartJobID);

                    if (smartJob != null)
                    {
                        JobKey jobKey = new JobKey(SmartJobID.ToString());

                        await _scheduler.DeleteJob(jobKey);

                        //Пишем лог
                        if (AllowLog)
                        {
                            await AddEvent(SmartJobID, "Task deleted");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                //Пишем лог
                await AddEvent(SmartJobID, "DeleteJob error: " + err.Message);
            }
        }

        //Добавляем запись в события
        private static async Task AddEvent(int SmartJobID, string Text)
        {
            using (var dbContext = new DatabaseContext())
            {
                var smartJob = await dbContext.SmartJobs.FirstOrDefaultAsync(x => x.ID == SmartJobID);

                var jobEvent = new JobEvent();
                jobEvent.IsSuccessed = true;
                jobEvent.RequestDate = DateTime.Now;
                jobEvent.SmartJob = smartJob;
                jobEvent.Text = Text;

                dbContext.JobEvents.Add(jobEvent);
                await dbContext.SaveChangesAsync();
            }
        }

        public class Smart
        {
            public string Method { get; set; }
        }

        public class JsonData
        {
            public string Amount { get; set; }
            public Smart Smart { get; set; }
            public string Source { get; set; }
            public string Target { get; set; }
            public string Priv { get; set; }

            public JsonData()
            {
                Smart = new Smart();
            }
        }

    }
}