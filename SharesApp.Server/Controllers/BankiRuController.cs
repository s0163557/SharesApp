using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SharesApp.Server.Classes;
using SharesApp.Server.Context;

namespace SharesApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankiRuController
    {
        [HttpGet("GetPageOfDividentsShare")]
        public async Task<string> GetPageOfDividentsShare(int pageNumber)
        {
            using (BankiRuHttpClient httpClient = new BankiRuHttpClient())
            {
                var response = await httpClient.GetAsync($"investment/shares/russian_shares/most-profit/?page={pageNumber}");
                var htmlText = await response.Content.ReadAsStringAsync();
                return htmlText;
            }
        }

        [HttpGet("GetNamesOfShares")]
        public async Task<List<string>> GetNamesOfShares()
        {
            List<string> shareNames = new List<string>();
            using (SecuritiesContext securitiesContext = new SecuritiesContext())
            {
                List<string> listOfSharesId = securitiesContext.SecurityInfos.Select(x => "_" + x.SecurityId).ToList();
                for (int i = 0; i < 30; i++)
                {
                    var htmlText = await GetPageOfDividentsShare(i);
                    for (int shareCounter = 0; shareCounter < listOfSharesId.Count; shareCounter++)
                    {
                        int indexOfShare = htmlText.IndexOf(listOfSharesId[shareCounter]);
                        if (indexOfShare != -1)
                        {
                            StringBuilder sb = new StringBuilder(listOfSharesId[shareCounter]);
                            indexOfShare--;
                            while (htmlText[indexOfShare] != '/')
                            {
                                sb = sb.Insert(0, htmlText[indexOfShare]);
                                indexOfShare--;
                            }
                            shareNames.Add(sb.ToString());

                            //Удалим найденную акцию для ускорени поиска и избежания дублировани данных
                            listOfSharesId.RemoveAt(shareCounter);
                            shareCounter--;
                        }
                    }
                }
                return shareNames;
            }
        }

        public class Dividend
        {
            public DateOnly registry;
            public DateOnly dateOfPayment;
            public int period;
            public double dividend;
            public int income;
        }

        [HttpGet("GetInfoFromDividendsPage")]
        public async Task<List<List<Dividend>>> GetInfoFromDividendsPage()
        {
            List<string> shareNames = new List<string> { "EnelRossiya_ENRU" };
            List<List<Dividend>> accordingDividends = new List<List<Dividend>>();
            for (int shareCounter = 0; shareCounter < shareNames.Count; shareCounter++)
            {
                var dividentsTableInhtml = await GetDividentsTableHtml(shareNames[shareCounter]);
                //Нам пришел чистый html код таблицы, и чтобы 
                var doc = new HtmlDocument();
                doc.LoadHtml(dividentsTableInhtml);

                var specificTableNode = doc.DocumentNode.SelectSingleNode("//table[@class='Table__sc-1n08tcd-0 rUIGk']");

                //В любом случае добавим пустое значение и попытаемся распарсить данные
                accordingDividends.Add(new List<Dividend>());

                if (specificTableNode != null)
                {
                    var extractedDividends = ExtractDividendsFromTable(specificTableNode);
                    //Если таки получилось выгрузить данные, то сохраним их
                    if (extractedDividends != null)
                        accordingDividends[shareCounter] = extractedDividends;
                }
            }

            return accordingDividends;
        }

        [HttpGet("ExtractDividendsFromTable")]
        public List<Dividend> ExtractDividendsFromTable(HtmlNode tableNode)
        {
            var dividends = new List<Dividend>();
            var tableBody = tableNode.SelectSingleNode("tbody");
            var rows = tableBody.SelectNodes("tr");

            Regex integerRegex = new Regex("[^0-9.,]");

            if (rows != null)
                foreach (var row in rows)
                {
                    var currentRowCells = row.SelectNodes("td | th");
                    if (currentRowCells != null && currentRowCells.Count == 5)
                    {
                        DateOnly currentRegistry;
                        if (DateOnly.TryParse(integerRegex.Replace(currentRowCells[0].InnerText.Trim(), ""), out DateOnly parsedRegistry))
                            currentRegistry = parsedRegistry;
                        else
                            currentRegistry = DateOnly.MinValue;

                        DateOnly currentDateOfPayment;
                        if (DateOnly.TryParse(integerRegex.Replace(currentRowCells[1].InnerText.Trim(), ""), out DateOnly pasedDateOfPayment))
                            currentDateOfPayment = pasedDateOfPayment;
                        else
                            currentDateOfPayment = DateOnly.MinValue;

                        int currentPeriod;
                        if (int.TryParse(integerRegex.Replace(currentRowCells[2].InnerText.Trim(), ""), out int parsedPeriod))
                            currentPeriod = parsedPeriod;
                        else
                            currentPeriod = -1;

                        double currentDividend;
                        if (double.TryParse(integerRegex.Replace(currentRowCells[3].InnerText.Trim(), "").Replace(',', '.'), out double parsedDividend))
                            currentDividend = parsedDividend;
                        else
                            currentDividend = -1;

                        int currentIncome;
                        if (int.TryParse(integerRegex.Replace(currentRowCells[4].InnerText.Trim(), ""), out int parsedIncome))
                            currentIncome = parsedIncome;
                        else
                            currentIncome = -1;

                        var dividend = new Dividend
                        {
                            registry = currentRegistry,
                            dateOfPayment = currentDateOfPayment,
                            period = currentPeriod,
                            dividend = currentDividend,
                            income = currentIncome
                        };

                        dividends.Add(dividend);
                    }
                }

            return dividends;
        }

        [HttpGet("GetDividentsTableHtml")]
        public async Task<string> GetDividentsTableHtml(string sharesName = "EnelRossiya_ENRU")
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            using (var driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl($"https://www.banki.ru/investment/share/{sharesName}/dividends/");
                var element = driver.FindElement(By.XPath("//span[@data-test='investment-dividends__show-all-link']"));
                IJavaScriptExecutor js = driver;
                js.ExecuteScript("arguments[0].click();", element);

                var tableHtml = driver.FindElement(By.XPath("//table[@class='Table__sc-1n08tcd-0 rUIGk']")).GetAttribute("outerHTML");

                return tableHtml;
            }
        }

    }
}
