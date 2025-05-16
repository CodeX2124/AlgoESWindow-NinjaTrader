using NinjaTrader.Cbi;

namespace AlgoESAddonWindow.SharedData
{
    public static class SharedDataESU
    {
        public static double LimitPrice { get; set; } = 0;
        public static int ContractCount { get; set; } = 0;
        public static string state { get; set; } = "";
        public static string orderID { get; set; }= "";
        public static Instrument instrument { get; set; } = new Instrument();
    }

    public static class SharedDataESZ
    {
        public static double LimitPrice { get; set; } = 0;
        public static int ContractCount { get; set; } = 0;
        public static string state { get; set; } = "";
        public static string orderID { get; set; } = "";
        public static Instrument instrument { get; set; } = new Instrument();
    }

    public static class SharedDataState
    {
        public static string[] States { get; set; } = new string[100];
        public static string[] orderIDs { get; set; } = new string[100];
        public static double[] Prices { get; set; } = new double[100];
        public static double[] limitPrices { get; set; } = new double[100];
        public static string[] states { get; set; } = new string[100];
        public static int cnt { get; set; } = 0;
    }   
}
