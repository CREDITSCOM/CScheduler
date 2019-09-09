using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CScheduler.Models
{
    public class EtherscanModel
    {
        public List<Holder> Holders { get; set; }
        public List<Transfer> Transfers { get; set; }

        public EtherscanModel()
        {
            Holders = new List<Holder>();
            Transfers = new List<Transfer>();
        }        
    }

    public class Holder
    {
        public string Rank { get; set; }
        public string Address { get; set; }
        public string Quantity { get; set; }
        public string Percentage { get; set; }
    }

    public class Transfer
    {
        public string Hash { get; set; }
        public string Age { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Quantity { get; set; }
    }
}