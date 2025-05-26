using System.Text;
using Microsoft.AspNetCore.Mvc;
using SharesApp.Server.Classes;
using SharesApp.Server.Context;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

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

        [HttpGet("GetInfoFromDividendsPage")]
        public async Task<string> GetInfoFromDividendsPage()
        {
            List<string> shareNames = new List<string> { "EnelRossiya_ENRU" };
            foreach (string shareName in shareNames)
            {
                var dividentsPage = await GetDividentsPage(shareName);

            }
        }

        [HttpGet("GetDividentsPage")]
        public async Task<string> GetDividentsPage(string sharesName = "EnelRossiya_ENRU")
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            using (var driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl($"https://www.banki.ru/investment/share/{sharesName}/dividends/");
                var element = driver.FindElement(By.XPath("//span[@data-test='investment-dividends__show-all-link']"));
                IJavaScriptExecutor js = driver;
                js.ExecuteScript("arguments[0].click();", element);
                var currentPage = driver.PageSource;
                return currentPage;
            }
        }

    }
}
