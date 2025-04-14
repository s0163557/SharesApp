using Stock_Analysis_Web_App.Classes;

namespace SharesApp.Server.Tools
{
    public class MoexStockDataHandler
    {

        public static List<MoexStockHistoryTrade> DeleteDuplicateRows(List<MoexStockHistoryTrade> listOfStocks)
        {
            //По какой-то причине MOEX хранит данные о нескольких торгах по одной бумаге в один день, с разным закрытием и открытием.
            //Удалим те записи, у которых меньше объем торгов.
            for (int i = 0; i < listOfStocks.Count(); i++)
            {

                for (int j = i; j < listOfStocks.Count(); j++)
                {
                    if (listOfStocks[i].SecId == listOfStocks[j].SecId)
                    {
                        //Нашил повторяющуюся запись о торгах по акции.
                        if (listOfStocks[i].Numtrades < listOfStocks[j].Numtrades)
                        {
                            //Если нашли больший элемент - меняем их местами
                            var swap = listOfStocks[i];
                            listOfStocks[i] = listOfStocks[j];
                            listOfStocks[j] = swap;
                        }
                        //Удаляем меньший элемент
                        listOfStocks.RemoveAt(j);
                    }
                }
            }
            return listOfStocks;
        }

        public static List<MoexStockHistoryTrade> DeleteRowsWithZeroes(List<MoexStockHistoryTrade> listOfStocks)
        {
            for (int i = 0; i < listOfStocks.Count; i++)
            {
                if (listOfStocks[i].Numtrades == 0)
                {
                    listOfStocks.Remove(listOfStocks[i]);
                    i--;
                }
            }

            return listOfStocks;
        }

    }
}
