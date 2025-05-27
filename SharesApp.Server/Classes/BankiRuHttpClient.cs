using Stock_Analysis_Web_App.Tools;

namespace SharesApp.Server.Classes
{
    public class BankiRuHttpClient : HttpClient
    {
        public BankiRuHttpClient() 
        {
            BaseAddress = new Uri(UrlQueryMaker.BaseBankiRuUrl);
        }
    }
}
