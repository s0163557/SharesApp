using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SharesApp.Server.Classes;
using SharesApp.Server.Models;

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

        private async Task<string> DownloadExcelFile()
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
                        return filePath.ToString();
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

        }

        private List<Inflation> ReadExcelFile(string pathToExcelFile)
        {
            List<Inflation> parsedInflations = new List<Inflation>();
            using (var package = new ExcelPackage(new FileInfo(pathToExcelFile)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                //Начинаем со второй строки потому что в первой идет объявление столбцов
                for (int rowCounter = 2; rowCounter < worksheet.Rows.Count(); rowCounter++)
                {
                    //Обычно в этих файлах три столбца - Дата, Ключевая ставка, Инфляция. Прочтём их и сохраним. 
                    Inflation currentRowInflation = new Inflation();

                    if (DateOnly.TryParse(worksheet.Cells[rowCounter, 1].Text, out DateOnly parsedDate))
                        currentRowInflation.DateOfRecord = parsedDate;
                    else
                    {
                        continue;
                    }

                    //KeyRate нам не очень важен, поэтмоу если он не сможет спарсится - не беда
                    currentRowInflation.KeyRate = -1;
                    if (decimal.TryParse(worksheet.Cells[rowCounter, 2].Text, out decimal parsedKeyRate))
                        currentRowInflation.KeyRate = parsedKeyRate;

                    //Если мы не смогли спарсить инфляцию - пропускаем это значение
                    if (decimal.TryParse(worksheet.Cells[rowCounter, 3].Text, out decimal parsedInflationValue))
                        currentRowInflation.InflationValue = parsedInflationValue;
                    else
                    {
                        continue;
                    }

                    parsedInflations.Add(currentRowInflation);
                }
                return parsedInflations;
            }

        }

        private string SaveRecordsInDb(List<Inflation> inflationRecords)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                try
                {
                    foreach (var inflation in inflationRecords)
                    {
                        //Проверим что в базе нет аналогичной записи
                        if (!dbContext.Inflations.Where(x => x.InflationValue == inflation.InflationValue
                                                        && x.KeyRate == inflation.KeyRate
                                                        && x.DateOfRecord == inflation.DateOfRecord).Any())
                            dbContext.Inflations.Add(inflation);
                    }
                    return dbContext.SaveChanges().ToString();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        [HttpGet("DownloadAndUploadData")]
        public async Task<IResult> DownloadAndUploadData()
        {
            try
            {
                string pathToFile = await DownloadExcelFile();
                List<Inflation> inflationRecords = ReadExcelFile(pathToFile);
                SaveRecordsInDb(inflationRecords);
                return Results.Accepted();
            }
            catch (Exception ex)
            {
                return Results.Conflict(ex);
            }

        }

    }
}
