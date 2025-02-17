using Stock_Analysis_Web_App.Tools;

namespace Stock_Analysis_Web_App.Classes
{
    public class MoexHttpClient : HttpClient
    {
        public MoexHttpClient()
        {
            BaseAddress = new Uri(UrlQueryMaker.BaseMoexIssUrl);
        }

    }
}
