using CScheduler.Models;
using DevRain.Data.Extracting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CScheduler.Controllers
{
    public class EtherscanController : Controller
    {
        public ActionResult Index()
        {
            var model = new EtherscanModel();

            //Holders
            for (int i = 1; i <= 14; i++)
            {
                var partHolders = GetPartHolders(i);
                model.Holders.AddRange(partHolders);
                //break;
            }

            //Transfers
            var transfers = GetTransfers();
            model.Transfers.AddRange(transfers);

            return View(model);
        }

        public List<Holder> GetPartHolders(int part)
        {
            var holders = new List<Holder>();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://etherscan.io/token/generic-tokenholders2?a=0x46b9ad944d1059450da1163511069c718f699d31&m=normal&s=249471071209990&p=" + part);
            //HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://etherscan.io/token/generic-tokentxns2?contractAddress=0x46b9ad944d1059450da1163511069c718f699d31&mode=&m=normal&p=" + part);
            req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            req.Headers.Add("Accept-Language", "en-US");
            req.Headers.Add("Cache-Control", "max-age=0");
            req.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763");
            //req.Headers.Add("Cookie", "ASP.NET_SessionId=4w1cceuu2qvume0izkluy4ue; __cfduid=d8ba8b8eefe8c4d571612bef9cbf76bc01567768581; __cflb=686120027");
            req.ContentType = "text/html; charset=utf-8";
            req.Referer = "https://etherscan.io/token/generic-tokenholders2?a=0x46b9ad944d1059450da1163511069c718f699d31&m=normal&s=249471071209990&p=" + (part-1);
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            req.Host = "etherscan.io";
            req.Method = "GET";
            req.Timeout = 10000;

            using (var resp = req.GetResponse())
            {
                using (var str = resp.GetResponseStream())
                using (var gsr = new GZipStream(str, CompressionMode.Decompress))
                using (var sr = new StreamReader(gsr))
                {
                    string responseString = sr.ReadToEnd();
                    HtmlProcessor proc = new HtmlProcessor(responseString);

                    DataTable table = proc.HtmlTables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        if (row.ItemArray[0].ToString() == "Rank")
                            continue;

                        holders.Add(new Holder()
                        {
                            Rank = row.ItemArray[0].ToString(),
                            Address = row.ItemArray[1].ToString().Replace("<SPAN>", "").Replace("</SPAN>", "").Replace("/token", "https://etherscan.io/token").Replace("target=_parent", "target=_blank"),
                            Quantity = ((row.ItemArray[2].ToString().Contains('.') ? row.ItemArray[2].ToString().Split('.')[0] : row.ItemArray[2].ToString())).Replace(",", " "),
                            Percentage = row.ItemArray[3].ToString()
                        });
                    }
                }
            }

            return holders;
        }

        public List<Transfer> GetTransfers()
        {
            var transfers = new List<Transfer>();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://etherscan.io/token/generic-tokentxns2?contractAddress=0x46b9ad944d1059450da1163511069c718f699d31&mode=&m=normal&p=1");
            req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            req.Headers.Add("Accept-Language", "en-US");
            req.Headers.Add("Cache-Control", "max-age=0");
            req.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763");
            //req.Headers.Add("Cookie", "ASP.NET_SessionId=4w1cceuu2qvume0izkluy4ue; __cfduid=d8ba8b8eefe8c4d571612bef9cbf76bc01567768581; __cflb=686120027");
            req.ContentType = "text/html; charset=utf-8";
            req.Referer = "https://etherscan.io/token/0x46b9ad944d1059450da1163511069c718f699d31#balances";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            req.Host = "etherscan.io";
            req.Method = "GET";
            req.Timeout = 10000;

            using (var resp = req.GetResponse())
            {
                using (var str = resp.GetResponseStream())
                using (var gsr = new GZipStream(str, CompressionMode.Decompress))
                using (var sr = new StreamReader(gsr))
                {
                    string responseString = sr.ReadToEnd();
                    HtmlProcessor proc = new HtmlProcessor(responseString);

                    DataTable table = proc.HtmlTables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        if (row.ItemArray[0].ToString() == "Txn Hash")
                            continue;

                        transfers.Add(new Transfer()
                        {
                            Hash = row.ItemArray[0].ToString().Replace("/tx", "https://etherscan.io/tx").Replace("_parent", "_blank"),
                            Age = row.ItemArray[1].ToString(),
                            From = row.ItemArray[2].ToString().Replace("0x46b9ad944d1059450da1163511069c718f699d31", "https://etherscan.io/token/0x46b9ad944d1059450da1163511069c718f699d31").Replace("_parent", "_blank"),
                            To = row.ItemArray[4].ToString().Replace("0x46b9ad944d1059450da1163511069c718f699d31", "https://etherscan.io/token/0x46b9ad944d1059450da1163511069c718f699d31").Replace("_parent", "_blank"),
                            Quantity = row.ItemArray[5].ToString()
                        });
                    }
                }
            }

            return transfers;
        }
    }
}