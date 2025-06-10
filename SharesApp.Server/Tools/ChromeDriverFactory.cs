using OpenQA.Selenium.Chrome;

namespace SharesApp.Server.Tools
{
    public class ChromeDriverFactory
    {
        public ChromeDriver CreateDriver()
        {

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            return new ChromeDriver(options);
        }

    }
}
