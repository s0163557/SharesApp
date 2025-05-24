using SharesApp.Server.Context;
using SharesApp.Server.Models;
using Stock_Analysis_Web_App.Classes;
using Stock_Analysis_Web_App.Classes.Converters;
using Stock_Analysis_Web_App.Tools;
using System.Net;

namespace FetchingDataService
{
    public class MoexDataReciever
    {
        MoexHttpClient MoexClient;

        HashSet<HttpStatusCode> _serverErrors = new HashSet<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };

        public MoexDataReciever(MoexHttpClient MoexClient)
        {
            this.MoexClient = MoexClient;
        }

        private async Task<MoexStockInfo> GetStockInfoFromMoex(string secId)
        {
            HttpResponseMessage response =
                await MoexClient.GetAsync(UrlQueryMaker.GetSecurityInfoUrl(secId));
            ResponseSerializer responseSerializer = new();
            MoexStockInfo stockInfo = await responseSerializer.DeserializeInfo<MoexStockInfo>(response, "description");
            return stockInfo;
        }

        public async Task<string> SendSecurityTradeRecordsToDb(List<MoexStockHistoryTrade> listOfStockHistoryTrades)
        {
            try
            {
                MoexToSecuritiesConverter moexToSecuritiesConverter = new MoexToSecuritiesConverter();
                List<SecurityTradeRecord> listOfSecurityTradeRecords = new List<SecurityTradeRecord>();

                using (SecuritiesContext securitiesDb = new SecuritiesContext())
                {
                    foreach (MoexStockHistoryTrade item in listOfStockHistoryTrades)
                    {
                        //Если запись о торгах пустая - пропустим её
                        if (item.Numtrades == 0)
                            continue;

                        //Если акция есть - добавялем к ней запись о торгах
                        SecurityInfo existingSecurity = null;
                        if (securitiesDb.SecurityInfos.Any(si => si.SecurityId == item.SecId))
                            existingSecurity = securitiesDb.SecurityInfos.First(si => si.SecurityId == item.SecId);

                        if (existingSecurity != null)
                        {
                            //Проверим, что такой же записи ещё нет в базе
                            var listOfExistingItems = securitiesDb.SecurityTradeRecords.Where(tr => tr.DateOfTrade == item.TradeDate && tr.SecurityInfo == existingSecurity).ToList();
                            if (!listOfExistingItems.Any())
                                securitiesDb.SecurityTradeRecords.Add(moexToSecuritiesConverter.ConvertMoexStockHistoryTradeToSecurityHistoryTrade(existingSecurity, item));
                        }
                        else
                        {
                            //Если акции нет - создаем её запись
                            var moexInfo = await GetStockInfoFromMoex(item.SecId);
                            SecurityInfo securityInfo = moexToSecuritiesConverter.ConvertMoexStockInfoToSecurityInfo(moexInfo);
                            securitiesDb.SecurityInfos.Add(securityInfo);
                            securitiesDb.SecurityTradeRecords.Add(moexToSecuritiesConverter.ConvertMoexStockHistoryTradeToSecurityHistoryTrade(securityInfo, item));
                            //Сохраним сейчас, чтобы случайно не добавить эту акцию во второй раз, если в выборке будут двойные строки
                            securitiesDb.SaveChanges();
                        }

                    }
                    securitiesDb.SaveChanges();
                }
                //Все прошло успешно, возвращаем True
                return "Успешно сохранены";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public async Task<List<MoexStockHistoryTrade>> GetStockHistoryTradesFromDate(DateOnly dateOfTrade)
        {
            //Поскольку в запросе выдается не больше 100 штук за раз, нам надо последовательно их запрашивать, пока количество возвращенных не будет меньше 100.
            int startIndex = 0;
            List<MoexStockHistoryTrade> listOfAllStocks = new List<MoexStockHistoryTrade>();
            List<MoexStockHistoryTrade> listOfPartStocks = new List<MoexStockHistoryTrade>();
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            ResponseSerializer serializer = new();
            do
            {
                int i = 0;
                do
                {
                    if (i > 0)
                    {
                        //Если это не первая наша попытка обратиться - делаем небольшую паузу
                        Thread.Sleep(500);
                        if (i >= 5)
                        {
                            //Если уже долгое время не можеи достучаться, то считаем сервер недоступным.
                            throw new Exception("Невозможно подключиться к сервису ISS Московской биржи");
                        }
                    }
                    response = await MoexClient.GetAsync(UrlQueryMaker.GetHistorySharesTradeInDateUrl(dateOfTrade, startIndex));
                    i++;
                }
                while (_serverErrors.TryGetValue(response.StatusCode, out var value));

                listOfPartStocks = await serializer.HttpResponseDeserializeToMoexStockHistoryTrade(response);
                listOfAllStocks.AddRange(listOfPartStocks);
                startIndex += listOfPartStocks.Count;
            }
            while (listOfPartStocks.Count > 0);
            return listOfAllStocks;
        }

    }
}
