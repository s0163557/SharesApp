using System.Globalization;

namespace Stock_Analysis_Web_App.Tools
{
    public class UrlQueryMaker
    {
        public static string HostIp = "185.105.226.205";
        public static string PortOfPostgresql = "5432";
        public static string DbUserName = "dbuser";
        public static string DbUserPassword = "Nemes341";
        public static string DbName = "securities";
        public static string BaseMoexIssUrl = "https://iss.moex.com/iss/";

        public static string GetDbConnectionUrl()
        {
            return "Host=" + HostIp + ";Port=" + PortOfPostgresql + ";Database=" + DbName + ";Username=" + DbUserName + ";Password=" + DbUserPassword;
        }

        public static string GetHistorySharesTradeInDateUrl(DateOnly dateOnly, int startIndex = 0)
        {
            return "history/engines/stock/markets/shares/securities.json?date=" + dateOnly.ToString("o", CultureInfo.InvariantCulture)
                + "&iss.only=history&iss.meta=off&start="+startIndex;
        }

        public static string GetSharesCollectionUrl(int startIndex = 0)
        {
            return "securitygroups/4/collections/stock_shares_all/securities.json?start="+startIndex;
        }

        public static string GetSecurityInfoUrl(string secId)
        {
            return "securities/" + secId + ".json?iss.meta=off";
        }

    }
}
