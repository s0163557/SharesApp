using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharesApp.Server.Classes;

namespace SharesApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CbrController
    {

        private DateOnly GetToday()
        { 
            return DateOnly.FromDateTime(DateTime.Now);
        }

        private string GetCurrentUrl()
        {
            //https://cbr.ru/Queries/UniDbQuery/DownloadExcel/132934?Posted=True&From=17.09.2013&To=28.05.2025&FromDate=09%2F17%2F2013&ToDate=05%2F28%2F2025
            DateOnly today = GetToday();
            string todayWithCorrectDots = today.ToString("MM/dd/yyyy").Replace(".", "%2F");
            string urlToFile = $"Queries/UniDbQuery/DownloadExcel/132934?Posted=True&From=17.09.2013&To={today}&FromDate=09%2F17%2F2013&ToDate={todayWithCorrectDots}";
            return urlToFile;
        }

        [HttpGet("DownloadExcelFile")]
        public async Task DownloadExcelFile()
        {
            using (CbrHttpClient httpClient = new CbrHttpClient())
            {
                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(GetCurrentUrl()))
                    {
                        response.EnsureSuccessStatusCode();

                        //Созадим заранее папку для загрузок
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Downloads");
                        StringBuilder filePath = new StringBuilder(Directory.GetCurrentDirectory());
                        filePath.Append("/Downloads/");
                        filePath.Append(Guid.NewGuid().ToString());
                        filePath.Append(" ");
                        filePath.Append(GetToday());
                        filePath.Append(".xlsx");

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                               fileStream = new FileStream(filePath.ToString(), FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }
                        Results.Accepted();
                    }
                }
                catch (Exception ex)
                {
                    Results.Conflict(ex);
                }
            }
        }

        public async ReadExcelFile()
        { 
            
        }

    }
}
