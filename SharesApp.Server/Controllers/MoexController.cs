using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SharesApp.Server.Context;
using SharesApp.Server.Models;
using SharesApp.Server.Tools;
using Stock_Analysis_Web_App.Classes;
using Stock_Analysis_Web_App.Classes.Converters;
using Stock_Analysis_Web_App.Tools;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;

namespace Stock_Analysis_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoexController : ControllerBase
    {
        MoexHttpClient MoexClient;

        HashSet<HttpStatusCode> _serverErrors = new HashSet<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };

        public MoexController(MoexHttpClient MoexClient)
        {
            this.MoexClient = MoexClient;
        }

        [HttpPost("SaveSharesToDb")]
        public void SendSharesToDb(List<MoexStockInfo> listOfInfos)
        {
            MoexToSecuritiesConverter converter = new MoexToSecuritiesConverter();
            using SecuritiesContext dbContext = new SecuritiesContext();
            {
                var infosInDb = dbContext.SecurityInfos.Select(o => o.SecurityId).ToHashSet();
                foreach (var info in listOfInfos)
                {
                    //Если в ДБ нет этой акции, то добавим её
                    if (!infosInDb.Contains(info.SecId))
                        dbContext.SecurityInfos.Add(converter.ConvertMoexStockInfoToSecurityInfo(info));
                }
            }
            dbContext.SaveChanges();
        }

        [HttpGet("GetSharesFromIss")]
        public async Task<List<MoexStockInfo>> GetListOfMoexStockInfos()
        {
            //Поскольку в запросе выдается не больше 100 штук за раз, нам надо последовательно их запрашивать, пока количество возвращенных не будет меньше 100.
            int startIndex = 0;
            List<MoexStockInfo> listOfAllShares = new List<MoexStockInfo>();
            List<MoexStockInfo> listOfPartShares = new List<MoexStockInfo>();
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
                            //Если уже долное время не можеи достучаться, то считаем сервер недоступным.
                            throw new Exception("Невозможно подключиться к сервису ISS Московской биржи");
                        }
                    }
                    response = await MoexClient.GetAsync(UrlQueryMaker.GetSharesCollectionUrl(startIndex));
                    i++;
                }
                while (_serverErrors.TryGetValue(response.StatusCode, out var value));

                listOfPartShares = await serializer.DeserializeList<MoexStockInfo>(response, "securities");
                listOfAllShares.AddRange(listOfPartShares);
                startIndex += listOfPartShares.Count;
            }
            while (listOfPartShares.Count > 0);

            //Иногда ISS выдает двойные строки - нам стоит их удалить
            HashSet<MoexStockInfo> visited = new HashSet<MoexStockInfo>();
            for (int i = 0; i < listOfAllShares.Count(); i++)
            {
                if (!visited.Add(listOfAllShares[i]))
                {
                    i--;
                    listOfAllShares.RemoveAt(i);
                }
            }

            return listOfAllShares;
        }

        [HttpPost("SendListOfStocksToDb")]
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

        [HttpGet("BusinessLogick")]
        public async Task<string> GetAndPostListOfStocks()
        {
            try
            {
                using (SecuritiesContext securitiesDb = new SecuritiesContext())
                {
                    securitiesDb.Database.EnsureDeleted();
                    securitiesDb.Database.EnsureCreated();

                    var list = await GetListOfMoexStockInfos();
                    try
                    {
                        SendSharesToDb(list);
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                    List<MoexStockHistoryTrade> listOfStocks = await GetStockHistoryTradesFromDate(new DateOnly(2025, 2, 3));
                    return await SendSecurityTradeRecordsToDb(listOfStocks);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<MoexStockInfo> GetStockInfoFromMoex(string secId)
        {
            HttpResponseMessage response =
                await MoexClient.GetAsync(UrlQueryMaker.GetSecurityInfoUrl(secId));
            ResponseSerializer responseSerializer = new();
            MoexStockInfo stockInfo = await responseSerializer.DeserializeInfo<MoexStockInfo>(response, "description");
            return stockInfo;
        }

        [HttpGet("GetStockHistoryTradesFromDate")]
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

        [HttpPost]
        public async Task<string> GetQuery(string query)
        {
            HttpResponseMessage response = await MoexClient.GetAsync(query);
            return await response.Content.ReadAsStringAsync();
        }

        [HttpGet]
        [Route("GetTradeRecordsInRangeOfDates")]
        public async Task<string> GetTradeRecordsInRangeOfDates(DateOnly from, DateOnly to)
        {
            List<StringBuilder> listOfErrors = new List<StringBuilder>();

            for (DateOnly currentDate = from; currentDate <= to; currentDate = currentDate.AddDays(1))
            {
                try
                {
                    List<MoexStockHistoryTrade> listOfStocks = await GetStockHistoryTradesFromDate(currentDate);
                    //Если в выбранный день не было торгов, то вернется пустой массив. 
                    if (listOfStocks.Count > 0)
                    {
                        listOfStocks = MoexStockDataHandler.DeleteDuplicateRows(listOfStocks);
                        listOfStocks = MoexStockDataHandler.DeleteRowsWithZeroes(listOfStocks);
                        await SendSecurityTradeRecordsToDb(listOfStocks);
                    }
                }
                catch (Exception ex)
                {
                    listOfErrors.Add(new StringBuilder("При загрузке от даты " + currentDate + " произошла ошибка:" + ex.Message));
                }
            }

            if (listOfErrors.Count == 0)
                return "Выгрузка данных за промежуток прошла успешно.";
            else
            {
                StringBuilder resultstring = new StringBuilder();
                foreach (StringBuilder error in listOfErrors)
                {
                    resultstring.Append(error.ToString());
                    resultstring.Append('\n');
                }
                return resultstring.ToString();
            }
        }

        [HttpGet]
        [Route("GatherDataIRnange")]
        public async Task<string> GatherDataInRange(int amountOfDaysInRange)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                foreach (var securityInfo in dbContext.SecurityInfos)
                {
                    //Выберем все данные о торгах по конкретной акции.
                    var listOfTrades = dbContext.SecurityTradeRecords.Where(x => x.SecurityInfoId == securityInfo.SecurityInfoId);

                    //Пройдемся по всем данным и соберем их по неделям.
                    List<List<SecurityTradeRecord>> securityTradeRecordByWeeks = new List<List<SecurityTradeRecord>>();
                    List<SecurityTradeRecord> securityTradeRecordInWeek = new List<SecurityTradeRecord>();
                    DateOnly endWeekDate = listOfTrades.First().DateOfTrade.AddDays(amountOfDaysInRange);
                    foreach (var tradeRecord in listOfTrades)
                    {
                        if (tradeRecord.DateOfTrade < endWeekDate)
                            securityTradeRecordInWeek.Add(tradeRecord);
                        else
                        {
                            //Если даты стали больше, значит мы прошли уже все возможные торги на неделе, и надо переходить к подсчету следующей
                            securityTradeRecordByWeeks.Add(securityTradeRecordInWeek);
                            securityTradeRecordInWeek.Clear();
                            endWeekDate = endWeekDate.AddDays(7);
                        }
                    }

                    List<SecurityTradeRecord> newListOfWeekTrades = new List<SecurityTradeRecord>();
                    //Собрали все данные по неделям, теперь сформируем на их основе новые данные.
                    foreach (var week in securityTradeRecordByWeeks)
                    {
                        if (week.Count > 0)
                        {
                            SecurityTradeRecord securityTradeRecord = new SecurityTradeRecord();
                            //найдем открывающие и закрывающие значения 
                            securityTradeRecord.Open = week.First().Open;
                            securityTradeRecord.Close = week.Last().Close;
                            securityTradeRecord.Low = week.Min(x => x.Low);
                            securityTradeRecord.High = week.Max(x => x.High);
                            //Найдем начало недели, если вдруг первый день не совпадает с первым днём первой записи
                            DateOnly dateTime = week.First().DateOfTrade;
                            while (dateTime.DayOfWeek != endWeekDate.DayOfWeek)
                                dateTime = dateTime.AddDays(-1);
                            securityTradeRecord.DateOfTrade = dateTime;
                            securityTradeRecord.NumberOfTrades = week.Sum(x => x.NumberOfTrades);
                            securityTradeRecord.Value = week.Sum(x => x.Value);
                            securityTradeRecord.SecurityInfo = week.First().SecurityInfo;
                            securityTradeRecord.SecurityInfoId = week.First().SecurityInfoId;
                            newListOfWeekTrades.Add(securityTradeRecord);
                        }
                    }

                    dbContext.SecurityTradeRecordsByWeek.AddRange(newListOfWeekTrades);
                    dbContext.SaveChanges();
                }
            }
            return "Сохранение прошло успешно";
        }

    }
}
