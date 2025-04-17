using Stock_Analysis_Web_App.Classes;
using Stock_Analysis_Web_App.Controllers;
using System.Diagnostics;

namespace SharesApp.Server.Tools
{
    public class MoexDataDayFetcher
    {
        TimeSpan HoursMiliseconds24 = new TimeSpan(24, 0, 0);

        public MoexDataDayFetcher()
        {
            DateTime fetchingTime = DateTime.Now.AddDays(1);

            //Поставим время на 1 час ночи
            fetchingTime = fetchingTime.AddHours(1 - fetchingTime.Hour);
            //Обнулим минуты и секунды
            fetchingTime = fetchingTime.AddMinutes(-fetchingTime.Minute);
            fetchingTime = fetchingTime.AddSeconds(-fetchingTime.Second);

            TimeSpan sleepTime = DateTime.Now - fetchingTime;
            FetchNewData(sleepTime);
        }

        async Task<string> FetchNewData(TimeSpan sleepTime)
        {
            await Task.Run(async () =>
            {
                try
                {
                    Debug.WriteLine("Ночное обновление данных за прошедший день");
                    Thread.Sleep(sleepTime);
                    using (MoexHttpClient httpClient = new MoexHttpClient())
                    {
                        MoexController moexController = new MoexController(httpClient);

                        var listOfStocks = await moexController.GetStockHistoryTradesFromDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-1)));
                        listOfStocks = MoexStockDataHandler.DeleteDuplicateRows(listOfStocks);
                        listOfStocks = MoexStockDataHandler.DeleteRowsWithZeroes(listOfStocks);
                        await moexController.SendSecurityTradeRecordsToDb(listOfStocks);
                        await FetchNewData(HoursMiliseconds24);
                    }
                    //Теперь надо проверить, что это начало месяца или завершение недели, и добавить туда новые данные

                    Debug.WriteLine("Успешно сохранил данные");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Не удалось загрузить данные за дату " + DateTime.Now.AddDays(-1) + ", возникла ошибка:");
                    Debug.WriteLine(ex);
                    await FetchNewData(HoursMiliseconds24);
                }
            });
            return "Процесс запущен";
        }

    }
}
