using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class SubscriptionsResponse
    {
        public List<Item> Items { get; set; }
        public PageInfo PageInfo { get; set; }
    }

    public class Item
    {
        public string SubscriberCode { get; set; }
        public int SubscriptionId { get; set; }
        public string Status { get; set; }
        public long AccessionDate { get; set; }
        public long EndAccessionDate { get; set; }
        public long RequestDate { get; set; }
        public long Date_Next_Charge { get; set; }
        public bool Trial { get; set; }
        public string Transaction { get; set; }
        public Plan Plan { get; set; }
        public Product Product { get; set; }
        public Price Price { get; set; }
        public Subscriber Subscriber { get; set; }
    }

    public class Plan
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int RecurrencyPeriod { get; set; }
        public int MaxChargeCycles { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ucode { get; set; }
    }

    public class Price
    {
        public double Value { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class Subscriber
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Ucode { get; set; }
    }

    public class PageInfo
    {
        public int TotalResults { get; set; }
        public string NextPageToken { get; set; }
        public string PrevPageToken { get; set; }
        public int ResultsPerPage { get; set; }
    }

}
