using SharesApp.Server.Tools;
using Stock_Analysis_Web_App.Classes;

namespace FetchingDataService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        
    }
}
