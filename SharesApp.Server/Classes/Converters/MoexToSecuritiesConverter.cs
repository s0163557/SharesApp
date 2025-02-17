using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Stock_Analysis_Web_App.Models;

namespace Stock_Analysis_Web_App.Classes.Converters
{
    public class MoexToSecuritiesConverter
    {

        public SecurityInfo ConvertMoexStockInfoToSecurityInfo(MoexStockInfo stockInfo)
        {
            if (stockInfo.SecId != null)
            {
                SecurityInfo securityInfo = new SecurityInfo();

                securityInfo.SecurityId = stockInfo.SecId;

                if (stockInfo.ListLevel != null)
                    securityInfo.ListLevel = (int)stockInfo.ListLevel;

                if (stockInfo.Name != null)
                    securityInfo.Name = stockInfo.Name;

                if (stockInfo.IssueDate != null)
                    securityInfo.IssueDate = (DateOnly)stockInfo.IssueDate;

                if (stockInfo.Isin != null)
                    securityInfo.Isin = stockInfo.Isin;

                if (stockInfo.IssueSize != null)
                    securityInfo.IssueSize = (int)stockInfo.IssueSize;

                return securityInfo;
            }
            else
            {
                throw new Exception("В переданном значении не было SecId");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockHistoryTrade">
        /// Необработанная запись о проведенных торгах
        /// </param>
        /// <param name="securityInfo">
        /// Та акция, к которой будет привязана запись о торгах 
        /// </param>
        /// <returns></returns>
        public SecurityTradeRecord ConvertMoexStockHistoryTradeToSecurityHistoryTrade(SecurityInfo requiredSecurytiInfo, MoexStockHistoryTrade stockHistoryTrade)
        {
            SecurityTradeRecord securityTradeRecord = new SecurityTradeRecord();

            securityTradeRecord.SecurityInfo = requiredSecurytiInfo;

            if (stockHistoryTrade.TradeDate != null)
                securityTradeRecord.DateOfTrade = (DateOnly)stockHistoryTrade.TradeDate;

            if (stockHistoryTrade.Numtrades != null)
                securityTradeRecord.NumberOfTrades = (int)stockHistoryTrade.Numtrades;

            if (stockHistoryTrade.Value != null)
                securityTradeRecord.Value = (double)stockHistoryTrade.Value;

            if (stockHistoryTrade.Open != null)
                securityTradeRecord.Open = (double)stockHistoryTrade.Open;

            if (stockHistoryTrade.Low != null)
                securityTradeRecord.Low = (double)stockHistoryTrade.Low;

            if (stockHistoryTrade.High != null)
                securityTradeRecord.High = (double)stockHistoryTrade.High;

            if (stockHistoryTrade.Close != null)
                securityTradeRecord.Close = (double)stockHistoryTrade.Close;

            return securityTradeRecord;
        }

    }
}
