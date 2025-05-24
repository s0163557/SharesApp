using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharesApp.Server.Context;
using SharesApp.Server.Models;

namespace SharesApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        [HttpGet]
        [Route("GetSecurities")]
        public List<SecurityInfo> GetListOfSecurities()
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                dbContext.SecurityInfos.Load();
                return dbContext.SecurityInfos.Select(x => x).ToList();
            }
        }

        [HttpGet]
        [Route("GetSecurity/{securityid}")]
        public SecurityInfo? GetSecurityInfo(string securityid)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                dbContext.SecurityInfos.Load();
                try
                {
                    SecurityInfo securityInfo = dbContext.SecurityInfos.Where(x => x.SecurityId == securityid).First();
                    return securityInfo;
                }
                catch (Exception ex)
                {
                    SecurityInfo sc = new SecurityInfo();
                    return sc;
                }
            }
        }

        [HttpGet]
        [Route("GetSecurityTradeRecords/{securityid}")]
        public string GetSecurityTradeRecordsForChart(string securityid)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                try
                {
                    List<SecurityTradeRecord> securityTradeRecords = dbContext.SecurityTradeRecords.Where(x => x.SecurityInfo.SecurityId == securityid).ToList();

                    var records = from record in securityTradeRecords
                                  select new
                                  {
                                      x = record.DateOfTrade,
                                      y = new List<double> { record.Open, record.High, record.Low, record.Close }
                                  };


                    return JsonConvert.SerializeObject(records, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
        }

        [HttpGet]
        [Route("GetSecurityTradeRecordsByDay/{securityid}")]
        public string GetSecurityTradeRecordsByDay(string securityid)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                try
                {
                    List<SecurityTradeRecord> securityTradeRecords = dbContext.SecurityTradeRecords.Where(x =>
                    x.SecurityInfo.SecurityId == securityid).OrderBy(x => x.DateOfTrade).ToList();
                    if (securityTradeRecords.Count > 30)
                        securityTradeRecords = securityTradeRecords.TakeLast(30).ToList();

                    var records = from record in securityTradeRecords
                                  select new
                                  {
                                      x = record.DateOfTrade,
                                      y = new List<double> { record.Open, record.High, record.Low, record.Close, record.NumberOfTrades }
                                  };

                    return JsonConvert.SerializeObject(records, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
        }

        [HttpGet]
        [Route("GetSecurityTradeRecordsByWeek/{securityid}")]
        public string GetSecurityTradeRecordsByWeek(string securityid)
        {
            try
            {
                DateOnly yearDelay = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
                using (SecuritiesContext dbContext = new SecuritiesContext())
                {
                    List<SecurityTradeRecordByWeek> securityTradeRecords = dbContext.SecurityTradeRecordsByWeek.Where(x =>
                    x.SecurityInfo.SecurityId == securityid &&
                    x.DateOfTrade >= yearDelay).ToList();

                    var records = from record in securityTradeRecords
                                  select new
                                  {
                                      x = record.DateOfTrade,
                                      y = new List<double> { record.Open, record.High, record.Low, record.Close, record.NumberOfTrades }
                                  };

                    return JsonConvert.SerializeObject(records, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        [HttpGet]
        [Route("GetSecurityTradeRecordsByMonth/{securityid}")]
        public string GetSecurityTradeRecordsByMonth(string securityid)
        {
            try
            {
                using (SecuritiesContext dbContext = new SecuritiesContext())
                {
                    List<SecurityTradeRecordByMonth> securityTradeRecords = dbContext.SecurityTradeRecordsByMonth.Where(x =>
                    x.SecurityInfo.SecurityId == securityid).ToList();

                    var records = from record in securityTradeRecords
                                  select new
                                  {
                                      x = record.DateOfTrade,
                                      y = new List<double> { record.Open, record.High, record.Low, record.Close, record.NumberOfTrades }
                                  };

                    return JsonConvert.SerializeObject(records, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return "";
            }

        }


    }
}
