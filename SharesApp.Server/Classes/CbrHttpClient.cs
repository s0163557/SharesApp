using Stock_Analysis_Web_App.Tools;

namespace SharesApp.Server.Classes
{
    public class CbrHttpClient : HttpClient
    {
        public CbrHttpClient() 
        {
            BaseAddress = new Uri(UrlQueryMaker.BaseCbrUrl);
        }
    }
}
