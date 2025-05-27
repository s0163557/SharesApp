using Microsoft.EntityFrameworkCore;
using SharesApp.Server.Models;
using SharesApp.Server.Tools;
using Stock_Analysis_Web_App.Classes;
using Stock_Analysis_Web_App.Controllers;
using System.Security.Permissions;

namespace SharesApp.Server.Services
{
    public class FetchingDataService : BackgroundService
    {

        private void UpdateTradesByWeek(string secId)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                //Найдем добавленную акцию по её ID
                SecurityInfo securityInfo = dbContext.SecurityInfos.Where(x => x.SecurityId == secId).FirstOrDefault();
                //получим последнюю неделю
                SecurityTradeRecordsByWeek tradeRecordByWeek = dbContext.SecurityTradeRecordsByWeeks.Where(x => x.SecurityInfo == securityInfo).Last();
                //Будем добавлять недели пока не перескочим текущую дату
                DateOnly currentWeekDateStart = tradeRecordByWeek.DateOfTrade;
                var dateTimeNow = DateOnly.FromDateTime(DateTime.Now);
                while (currentWeekDateStart <= dateTimeNow)
                    currentWeekDateStart.AddDays(7);
                //Отнимем неделю, потому что мы перескачили текущую дату
                currentWeekDateStart.AddDays(-7);

                //Выбирем все данные начиная с этой недели
                List<SecurityTradeRecord> week = dbContext.SecurityTradeRecords.Where(x => x.DateOfTrade < currentWeekDateStart && x.SecurityInfo == securityInfo).ToList();
                //Соберём итоговые данные 
                SecurityTradeRecordsByWeek securityTradeRecord = new SecurityTradeRecordsByWeek();
                securityTradeRecord.Open = week.First().Open;
                securityTradeRecord.Close = week.Last().Close;
                securityTradeRecord.Low = week.Min(x => x.Low);
                securityTradeRecord.High = week.Max(x => x.High);
                DateOnly dateTime = week.First().DateOfTrade;
                securityTradeRecord.DateOfTrade = currentWeekDateStart;
                securityTradeRecord.NumberOfTrades = week.Sum(x => x.NumberOfTrades);
                securityTradeRecord.Value = week.Sum(x => x.Value);
                securityTradeRecord.SecurityInfo = week.First().SecurityInfo;
                securityTradeRecord.SecurityInfoId = week.First().SecurityInfoId;

                //Сформировали данные. Обновим старую запись, если она есть.
                if (dbContext.SecurityTradeRecordsByWeeks.Where(x => x.DateOfTrade == currentWeekDateStart &&  x.SecurityInfo == securityInfo) != null)
                    securityTradeRecord.SecurityTradeRecordId = week.First().SecurityTradeRecordId;
                dbContext.SecurityTradeRecordsByWeeks.Update(securityTradeRecord);
                dbContext.SaveChanges();
            }
        }

        private void UpdateTradesByMonth(string secId)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                //Найдем добавленную акцию по её ID
                SecurityInfo securityInfo = dbContext.SecurityInfos.Where(x => x.SecurityId == secId).FirstOrDefault();
                //получим последнюю неделю
                SecurityTradeRecordsByMonth tradeRecordByMonth = dbContext.SecurityTradeRecordsByMonths.Where(x => x.SecurityInfo == securityInfo).Last();
                //Будем добавлять недели пока не перескочим текущую дату
                var dateTimeNow = DateOnly.FromDateTime(DateTime.Now);
                DateOnly currentMonthDateStart = tradeRecordByMonth.DateOfTrade;
                while (currentMonthDateStart <= dateTimeNow)
                    currentMonthDateStart.AddMonths(1);
                //Отнимем месяц, потому что мы перескачили текущую дату
                currentMonthDateStart.AddMonths(-1);

                //Выбирем все данные начиная с этой недели
                List<SecurityTradeRecord> month = dbContext.SecurityTradeRecords.Where(x => x.DateOfTrade <= currentMonthDateStart && x.SecurityInfo == securityInfo).ToList();
                //Соберём итоговые данные 
                SecurityTradeRecordsByMonth securityTradeRecord = new SecurityTradeRecordsByMonth();
                securityTradeRecord.Open = month.First().Open;
                securityTradeRecord.Close = month.Last().Close;
                securityTradeRecord.Low = month.Min(x => x.Low);
                securityTradeRecord.High = month.Max(x => x.High);
                DateOnly dateTime = month.First().DateOfTrade;
                securityTradeRecord.DateOfTrade = currentMonthDateStart;
                securityTradeRecord.NumberOfTrades = month.Sum(x => x.NumberOfTrades);
                securityTradeRecord.Value = month.Sum(x => x.Value);
                securityTradeRecord.SecurityInfo = month.First().SecurityInfo;
                securityTradeRecord.SecurityInfoId = month.First().SecurityInfoId;

                //Сформировали данные. Обновим старую запись, если она есть.
                if (dbContext.SecurityTradeRecordsByMonths.Where(x => x.DateOfTrade == currentMonthDateStart && x.SecurityInfo == securityInfo) != null)
                    securityTradeRecord.SecurityTradeRecordId = month.First().SecurityTradeRecordId;
                dbContext.SecurityTradeRecordsByMonths.Update(securityTradeRecord);
                dbContext.SaveChanges();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime fetchingTime = DateTime.Now.AddDays(1);

                //Поставим время на 1 час ночи
                fetchingTime = fetchingTime.AddHours(1 - fetchingTime.Hour);
                //Обнулим минуты и секунды
                fetchingTime = fetchingTime.AddMinutes(-fetchingTime.Minute);
                fetchingTime = fetchingTime.AddSeconds(-fetchingTime.Second);

                try
                {
                    Console.WriteLine("Ночное обновление данных за прошедший день");
                    Thread.Sleep(1);

                    //Сохраним номера акцйи дял упрощения будуще йагрегации по неделям и месяцам
                    List<string?> downloadedSecIds = new List<string?>();
                    using (MoexHttpClient httpClient = new MoexHttpClient())
                    {
                        MoexController moexController = new MoexController(httpClient);

                        var listOfStocks = await moexController.GetStockHistoryTradesFromDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));
                        listOfStocks = MoexStockDataHandler.DeleteDuplicateRows(listOfStocks);
                        listOfStocks = MoexStockDataHandler.DeleteRowsWithZeroes(listOfStocks);
                        downloadedSecIds = listOfStocks.Select(x => x.SecId).ToList();
                        await moexController.SendSecurityTradeRecordsToDb(listOfStocks);
                    }

                    //Теперь надо проверить, что это начало месяца или завершение недели, и добавить туда новые данные
                    foreach (string secId in downloadedSecIds)
                    {
                        //Соберём данные по неделям
                        UpdateTradesByWeek(secId);
                        UpdateTradesByMonth(secId);
                    }

                    Console.WriteLine("Успешно сохранил данные");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось загрузить данные за дату " + DateTime.Now.AddDays(-1) + ", возникла ошибка:");
                    Console.WriteLine(ex);
                }

                TimeSpan sleepTime = fetchingTime - DateTime.Now;
                await Task.Delay((int)sleepTime.TotalMilliseconds, stoppingToken);
            }
        }
    }
}
