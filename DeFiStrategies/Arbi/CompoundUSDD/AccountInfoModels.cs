namespace DeFi_Strategies.Tron.CompoundRDNT
{
    public class AccountInfoRoot
    {
        public Datum[] data { get; set; }
        public bool success { get; set; }
    }

    public class Datum
    {
        public string chain { get; set; }
        public string id { get; set; }
        public string symbol { get; set; }
        public Decimal balance { get; set; }
        public Decimal price { get; set; }
        public string name { get; set; }
    }
}
