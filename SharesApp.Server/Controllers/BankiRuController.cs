using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SharesApp.Server.Classes;
using SharesApp.Server.Models;

namespace SharesApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankiRuController
    {
        [HttpGet("GetPageOfDividentsShare")]
        private async Task<string> GetPageOfDividentsShare(int pageNumber)
        {
            using (BankiRuHttpClient httpClient = new BankiRuHttpClient())
            {
                var response = await httpClient.GetAsync($"investment/shares/russian_shares/most-profit/?page={pageNumber}");
                var htmlText = await response.Content.ReadAsStringAsync();
                return htmlText;
            }
        }

        [HttpPost("SaveDividendInDatabase")]
        public int SaveDividendInDatabase(SecurityDividend dividendToSave)
        {
            using SecuritiesContext securitiesContext = new SecuritiesContext();
            {
                //Добавим запись только если нет аналогичной
                if (!securitiesContext.SecurityDividends.Where(x =>
                x.Income == dividendToSave.Income &&
                x.Dividend == dividendToSave.Dividend &&
                x.Period == dividendToSave.Period &&
                x.Registry == dividendToSave.Registry &&
                x.DateOfPayment == dividendToSave.DateOfPayment &&
                x.SecurityInfo == dividendToSave.SecurityInfo
                ).Any())
                    securitiesContext.SecurityDividends.Add(dividendToSave);

                return securitiesContext.SaveChanges();

            }
        }

        [HttpPost("SaveDividendsInDatabase")]
        public int SaveDividendsInDatabase(IEnumerable<SecurityDividend> dividendsToSave)
        {
            using SecuritiesContext securitiesContext = new SecuritiesContext();
            {
                foreach (var dividend in dividendsToSave)
                {
                    //Добавим запись только если нет аналогичной
                    if (!securitiesContext.SecurityDividends.Where(x =>
                    x.Income == dividend.Income &&
                    x.Dividend == dividend.Dividend &&
                    x.Period == dividend.Period &&
                    x.Registry == dividend.Registry &&
                    x.DateOfPayment == dividend.DateOfPayment &&
                    x.SecurityInfo == dividend.SecurityInfo
                    ).Any())
                        securitiesContext.SecurityDividends.Add(dividend);
                }

                return securitiesContext.SaveChanges();
            }
        }


        [HttpGet("GetNamesOfShares")]
        private async Task<Dictionary<string, SecurityInfo>> GetNamesOfShares()
        {
            Dictionary<string, SecurityInfo> shareNamesSecurityInfoDictionary = new Dictionary<string, SecurityInfo>();
            using (SecuritiesContext securitiesContext = new SecuritiesContext())
            {
                List<string> listOfSharesId = securitiesContext.SecurityInfos.Select(x => "_" + x.SecurityId).ToList();
                for (int i = 0; i < 25; i++)
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
                            //Добавим соответствие между найденным название и записью об акции в бд
                            var stringOfSb = sb.ToString();
                            var ssecurityInfo = securitiesContext.SecurityInfos.First(x => x.SecurityId == listOfSharesId[shareCounter].Remove(0, 1));
                            shareNamesSecurityInfoDictionary.Add(stringOfSb, ssecurityInfo);

                            //Удалим найденную акцию для ускорени поиска и избежания дублировани данных
                            listOfSharesId.RemoveAt(shareCounter);
                            shareCounter--;
                        }
                    }
                }
                return shareNamesSecurityInfoDictionary;
            }
        }

        [HttpPost("GetAndSaveDividentsInfo")]
        public async Task<IResult> GetInfoFromDividendsPage()
        {
            try
            {
                Dictionary<string, SecurityInfo> shareNamesSecurityInfoDictionary = await GetNamesOfShares();
                List<List<SecurityDividend>> accordingDividends = new List<List<SecurityDividend>>();
                for (int shareCounter = 0; shareCounter < shareNamesSecurityInfoDictionary.Count; shareCounter++)
                {
                    string dividentNameInBankRu = shareNamesSecurityInfoDictionary.Keys.ElementAt(shareCounter);
                    var dividentsTableInhtml = GetDividentsTableHtml(dividentNameInBankRu);
                    //Нам пришел чистый html код таблицы, и чтобы 
                    var doc = new HtmlDocument();
                    doc.LoadHtml(dividentsTableInhtml);

                    var specificTableNode = doc.DocumentNode.SelectSingleNode("//table[@class='Table__sc-1n08tcd-0 rUIGk']");

                    //В любом случае добавим пустое значение и попытаемся распарсить данные
                    accordingDividends.Add(new List<SecurityDividend>());

                    if (specificTableNode != null)
                    {
                        var extractedDividends = ExtractDividendsFromTable(specificTableNode, shareNamesSecurityInfoDictionary[dividentNameInBankRu]);
                        //Если таки получилось выгрузить данные, то сохраним их
                        if (extractedDividends != null)
                            accordingDividends[shareCounter] = extractedDividends;
                    }
                }
                List<SecurityDividend> allDividendsInOneList = new List<SecurityDividend>();
                foreach (var dividendsList in accordingDividends)
                    allDividendsInOneList.AddRange(dividendsList);

                SaveDividendsInDatabase(allDividendsInOneList);
                return Results.Accepted();
            }
            catch (Exception ex)
            {
                return Results.Conflict(ex);
            }
        }

        [HttpGet("ExtractDividendsFromTable")]
        private List<SecurityDividend> ExtractDividendsFromTable(HtmlNode tableNode, SecurityInfo attachedSecurityInfo)
        {
            var dividends = new List<SecurityDividend>();
            var tableBody = tableNode.SelectSingleNode("tbody");
            var rows = tableBody.SelectNodes("tr");

            Regex integerRegex = new Regex("[^0-9.,]");

            if (rows != null)
                foreach (var row in rows)
                {
                    var currentRowCells = row.SelectNodes("td | th");
                    if (currentRowCells != null && currentRowCells.Count == 5)
                    {
                        DateOnly currentRegistry = DateOnly.MinValue;
                        if (DateOnly.TryParse(integerRegex.Replace(currentRowCells[0].InnerText.Trim(), ""), out DateOnly parsedRegistry))
                            currentRegistry = parsedRegistry;

                        DateOnly currentDateOfPayment = DateOnly.MinValue;
                        if (DateOnly.TryParse(integerRegex.Replace(currentRowCells[1].InnerText.Trim(), ""), out DateOnly pasedDateOfPayment))
                            currentDateOfPayment = pasedDateOfPayment;

                        DateOnly currentPeriod = DateOnly.MinValue;
                        if (int.TryParse(integerRegex.Replace(currentRowCells[2].InnerText.Trim(), ""), out int parsedPeriod))
                            currentPeriod = new DateOnly(parsedPeriod, 1, 1);

                        double currentDividend = -1;
                        if (double.TryParse(integerRegex.Replace(currentRowCells[3].InnerText.Trim(), "").Replace(',', '.'), out double parsedDividend))
                            currentDividend = parsedDividend;

                        decimal currentIncome = -1;
                        if (decimal.TryParse(integerRegex.Replace(currentRowCells[4].InnerText.Trim(), ""), out decimal parsedIncome))
                            currentIncome = parsedIncome / 100;

                        var dividend = new SecurityDividend
                        {
                            Registry = currentRegistry,
                            DateOfPayment = currentDateOfPayment,
                            Period = currentPeriod,
                            Dividend = currentDividend,
                            Income = currentIncome,
                            SecurityInfo = attachedSecurityInfo,
                        };

                        dividends.Add(dividend);
                    }
                }

            return dividends;
        }

        [HttpGet("GetDividentsTableHtml")]
        public string GetDividentsTableHtml(string sharesName = "EnelRossiya_ENRU")
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            try
            {
                using (var driver = new ChromeDriver(options))
                {
                    driver.Navigate().GoToUrl($"https://www.banki.ru/investment/share/{sharesName}/dividends/");
                    var element = driver.FindElement(By.XPath("//span[@data-test='investment-dividends__show-all-link']"));
                    IJavaScriptExecutor js = driver;
                    js.ExecuteScript("arguments[0].click();", element);

                    var tableHtml = driver.FindElement(By.XPath("//table[@class='Table__sc-1n08tcd-0 rUIGk']")).GetAttribute("outerHTML");
                    driver.Quit();

                    return tableHtml;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

    }
}
