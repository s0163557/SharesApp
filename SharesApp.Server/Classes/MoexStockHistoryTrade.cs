using System.Runtime.Serialization;

namespace Stock_Analysis_Web_App.Classes
{
    public class MoexStockHistoryTrade
    {
        //Все имена переменных представлены в том же виде, в каком они представлены в получаемом JSON'e
        public string? SecId;
        public DateOnly? TradeDate;
        public int? Numtrades;
        public double? Value;
        public double? Open;
        public double? Low;
        public double? High;
        public double? Close;
    }
}
