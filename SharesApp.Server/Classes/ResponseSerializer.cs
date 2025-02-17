using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace Stock_Analysis_Web_App.Classes
{
    public class ResponseSerializer
    {
        public async Task<List<T>> DeserializeData<T>(HttpResponseMessage response, string dataName)
        {
            /* Нам необходимо получить json с сервера, десериализовать его, и отправить нормальные данные.
             * В чем проблемы?
             * Сами данные присылаются в виде строки, столбцы которые их описывают присылаются в виде отдельной строки, надо их сопоставить.
             * Не хочется каждый раз, меня класс ищущий присылаемые данные, лезть сюда и разбираться кдуа что вставлять.
             */
            StringBuilder stringBuilder = new StringBuilder(await response.Content.ReadAsStringAsync());
            JObject parsedResponse = JObject.Parse(stringBuilder.ToString());
            //В присылаемом ответе лежит объект history, хранящий в себе массивы data и columns. Запишем их.

            //data хранит основные данные об акциях, которые нам нужны.
            JToken dataToken = parsedResponse[dataName]["data"];
            List<JToken> listOfObjects = JsonConvert.DeserializeObject<List<JToken>>(dataToken.ToString());

            //columns хранит информацию о столбцах и их порядке.
            JToken columnsToken = parsedResponse[dataName]["columns"];
            List<string> listOfColumnNames = JsonConvert.DeserializeObject<List<string>>(columnsToken.ToString());

            //Получаем индексы полей, чтобы потом их не считать
            Dictionary<string, int> indexesOfColumns = new Dictionary<string, int>();
            for (int i = 0; i < listOfColumnNames.Count(); i++)
                indexesOfColumns.Add(listOfColumnNames[i], i);

            //Получаем список имен полей переданного типач
            List<string> listOfFields = typeof(T).GetFields()
                            .Select(field => field.Name)
                            .ToList();

            List<T> listOfDeserializedObjects = new List<T>();

            //Проходим по всем записям данных и отдельно создаем новые объекты акций, записывая в низ данные по именам их полей.
            foreach (JToken unserializedStock in dataToken)
            {
                T currentObject = (T)Activator.CreateInstance(typeof(T));
                foreach (string nameOfField in listOfFields)
                {
                    // Получаем инстанс конкретного поля по его имени
                    FieldInfo fi = currentObject.GetType().GetField(nameOfField, BindingFlags.Public | BindingFlags.Instance);
                    // Присваиваем значение полю по его имени
                    fi.SetValue(currentObject, Convert.ChangeType(unserializedStock[indexesOfColumns[nameOfField.ToUpper()]], fi.FieldType));

                }
                listOfDeserializedObjects.Add(currentObject);
            }
            return listOfDeserializedObjects;
        }

        public async Task<T> DeserializeInfo<T>(HttpResponseMessage response, string dataName)
        {
            /* Нам необходимо получить данные, которые описывают конкретную акцию.
             * Когда мы запрашиваем info, мы получаем и описание столбцов и значения в них, поэтмоу нам нужна немного другая логика
             * Два главных параметра, которые нас интересуюьт - name(название столбца) и value(значение столбца).
             * Нам нужно найти индексы name и value, считать из них искомые поля, и затем обратиться к их value - и записать его.00
             */
            string nameField = "name", valueField = "value";
            StringBuilder stringBuilder = new StringBuilder(await response.Content.ReadAsStringAsync());
            JObject parsedResponse = JObject.Parse(stringBuilder.ToString());
            //В присылаемом ответе лежит объект history, хранящий в себе массивы data и columns. Запишем их.

            //data хранит основные данные об акциях, которые нам нужны.
            JToken dataToken = parsedResponse[dataName]["data"];
            List<JToken> listOfObjects = JsonConvert.DeserializeObject<List<JToken>>(dataToken.ToString());

            //columns хранит информацию о столбцах и их порядке.
            JToken columnsToken = parsedResponse[dataName]["columns"];
            List<string> listOfColumnNames = JsonConvert.DeserializeObject<List<string>>(columnsToken.ToString());

            //Получаем индексы полей, чтобы потом их не считать
            Dictionary<string, int> indexesOfColumns = new Dictionary<string, int>();
            for (int i = 0; i < listOfColumnNames.Count(); i++)
                indexesOfColumns.Add(listOfColumnNames[i], i);

            //Получаем список имен полей переданного типач
            List<string> listOfFields = typeof(T).GetFields()
                            .Select(field => field.Name)
                            .ToList();

            List<T> listOfDeserializedObjects = new List<T>();

            //Проходим по всем записям данных и отдельно создаем новые объекты акций, записывая в них данные по именам их полей.
            T currentObject = (T)Activator.CreateInstance(typeof(T));
            foreach (JToken unserializedResponseFieldName in dataToken)
            {
                //Поскольку имена полей класса не находся в UPPERCASE, и мы не можем для проверки перевести их в UPPERCASE,
                //нам придется каждый раз перепроверять каждый из них ручками
                foreach (string nameOfClassField in listOfFields)
                {
                    //Проверяем, совпали ли имена
                    if (nameOfClassField.ToUpper() == unserializedResponseFieldName[indexesOfColumns[nameField]].ToString())
                    {
                        // Получаем инстанс нашего поля по совпавшему имени
                        FieldInfo fi = currentObject.GetType().GetField(nameOfClassField, BindingFlags.Public | BindingFlags.Instance);
                        // Присваиваем значение полю по его имени
                        fi.SetValue(currentObject, Convert.ChangeType(unserializedResponseFieldName[indexesOfColumns[valueField]], fi.FieldType));
                    }
                }
            }
            return currentObject;
        }

        public async Task<List<T>> DeserializeList<T>(HttpResponseMessage response, string dataName)
        {
            StringBuilder stringBuilder = new StringBuilder(await response.Content.ReadAsStringAsync());
            JObject parsedResponse = JObject.Parse(stringBuilder.ToString());
            //В присылаемом ответе лежит объект history, хранящий в себе массивы data и columns. Запишем их.

            //data хранит основные данные об акциях, которые нам нужны.
            JToken historyToken = parsedResponse[dataName]["data"];
            List<JToken> listOfObjects = JsonConvert.DeserializeObject<List<JToken>>(historyToken.ToString());

            //columns хранит информацию о столбцах и их порядке.
            JToken columnsToken = parsedResponse[dataName]["columns"];
            List<string> listOfColumnNames = JsonConvert.DeserializeObject<List<string>>(columnsToken.ToString());

            //Получаем индексы полей, чтобы потом их не считать
            Dictionary<string, int> indexesOfColumns = new Dictionary<string, int>();
            for (int i = 0; i < listOfColumnNames.Count(); i++)
                indexesOfColumns.Add(listOfColumnNames[i], i);

            //Получаем список имен полей нашей акции
            List<string> listOfVariables = typeof(T).GetFields()
                            .Select(field => field.Name)
                            .ToList();

            List<T> listOfData = new List<T>();

            //Проходим по всем записям данных и отдельно создаем новые объекты акций, записывая в них данные по именам их полей.
            foreach (JToken unserializedStock in historyToken)
            {
                T currentObject = (T)Activator.CreateInstance(typeof(T));
                foreach (string nameOfVariable in listOfVariables)
                {
                    // Получаем инстанс конкретного поля по его имени
                    FieldInfo fi = currentObject.GetType().GetField(nameOfVariable, BindingFlags.Public | BindingFlags.Instance);
                    // Присваиваем значение полю по его имени
                    fi.SetValue(currentObject, Convert.ChangeType(unserializedStock[indexesOfColumns[nameOfVariable.ToUpper()]], fi.FieldType));
                }
                listOfData.Add(currentObject);
            }

            return listOfData;
        }

            public async Task<List<MoexStockHistoryTrade>> HttpResponseDeserializeToMoexStockHistoryTrade(HttpResponseMessage response)
        {
            /* Нам необходимо получить json с сервера, десериализовать его, и отправить нормальные данные.
             * В чем проблемы?
             * Сами данные присылаются в виде строки, столбцы которые их описывают присылаются в виде отдельной строки, надо их сопоставить.
             * Не хочется каждый раз, меня класс ищущий присылаемые данные, лезть сюда и разбираться кдуа что вставлять.
             */
            StringBuilder stringBuilder = new StringBuilder(await response.Content.ReadAsStringAsync());
            JObject parsedResponse = JObject.Parse(stringBuilder.ToString());
            //В присылаемом ответе лежит объект history, хранящий в себе массивы data и columns. Запишем их.

            //data хранит основные данные об акциях, которые нам нужны.
            JToken historyToken = parsedResponse["history"]["data"];
            List<JToken> listOfObjects = JsonConvert.DeserializeObject<List<JToken>>(historyToken.ToString());

            //columns хранит информацию о столбцах и их порядке.
            JToken columnsToken = parsedResponse["history"]["columns"];
            List<string> listOfColumnNames = JsonConvert.DeserializeObject<List<string>>(columnsToken.ToString());

            //Получаем индексы полей, чтобы потом их не считать
            Dictionary<string, int> indexesOfColumns = new Dictionary<string, int>();
            for (int i = 0; i < listOfColumnNames.Count(); i++)
                indexesOfColumns.Add(listOfColumnNames[i], i);

            //Получаем список имен полей нашей акции
            List<string> listOfVariables = typeof(MoexStockHistoryTrade).GetFields()
                            .Select(field => field.Name)
                            .ToList();

            List<MoexStockHistoryTrade> listOfStocks = new List<MoexStockHistoryTrade>();

            //Проходим по всем записям данных и отдельно создаем новые объекты акций, записывая в них данные по именам их полей.
            foreach (JToken unserializedStock in historyToken)
            {
                MoexStockHistoryTrade moexStock = new MoexStockHistoryTrade();
                foreach (string nameOfVariable in listOfVariables)
                {
                    // Получаем инстанс конкретного поля по его имени
                    FieldInfo fi = moexStock.GetType().GetField(nameOfVariable, BindingFlags.Public | BindingFlags.Instance);
                    // Присваиваем значение полю по его имени
                    fi.SetValue(moexStock, Convert.ChangeType(unserializedStock[indexesOfColumns[nameOfVariable.ToUpper()]], fi.FieldType));

                }
                listOfStocks.Add(moexStock);
            }
            return listOfStocks;

        }

        public string SerializeListMoexStockHistoryTradeToJson(IEnumerable<MoexStockHistoryTrade> listOfStockHistoryTrades)
        {
            return JsonConvert.SerializeObject(listOfStockHistoryTrades);
        }

        public string SerializeListMoexStockInfoToJson(IEnumerable<MoexStockInfo> listOfStockInfos)
        {
            return JsonConvert.SerializeObject(listOfStockInfos);
        }

        public string SerializeMoexStockInfoToJson(MoexStockInfo stockInfo)
        {
            return JsonConvert.SerializeObject(stockInfo);
        }
    }
}
